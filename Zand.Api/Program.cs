using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.MappingProfiles;
using HotelListing.Api.Application.Services;
using HotelListing.Api.CachePolicies;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Config;
using HotelListing.Api.Data;
using HotelListing.Api.Domain;
using HotelListing.Api.Filters;
using HotelListing.Api.Handler;
using HotelListing.Api.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HotelListing API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Log.Fatal("Connection string 'HotelListingDbConnectionString' is not configured");
        throw new InvalidOperationException("Connection string 'HotelListingDbConnectionString' is not configured.");
    }

    builder.Services.AddDbContextPool<HotelListingDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    }, poolSize: 128);

    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<HotelListingDbContext>();

    builder.Services.AddHttpContextAccessor();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        Log.Fatal("JwtSettings:Key is not configured");
        throw new InvalidOperationException("JwtSettings:Key is not configured.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = context.ErrorDescription ?? "Authentication is required to access this resource.",
                    Instance = context.Request.Path
                };

                return context.Response.WriteAsJsonAsync(problemDetails);
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        AuthenticationDefaults.BasicScheme,
        _ => { })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAunthenticationHandler>(
        AuthenticationDefaults.ApiKeyScheme,
        _ => { });

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<ICountriesService, CountriesService>();
    builder.Services.AddScoped<IHotelsService, HotelsService>();
    builder.Services.AddScoped<IUsersService, UsersService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddMaps(typeof(HotelMappingProfile).Assembly);
    });

    builder.Services.AddControllers()
        .AddNewtonsoftJson()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    builder.Services.AddOpenApi();

    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy(CacheConstants.AuthenticatedUserCachingPolicy, policyBuilder =>
        {
            policyBuilder
                .AddPolicy<AuthenticatedUserCachingPolicy>()
                .SetCacheKeyPrefix(CacheConstants.AuthenticatedUserCachingPolicyTag);
        }, true);
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(RateLimitingConstants.FixedPolicy, limiterOptions =>
        {
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.PermitLimit = 50;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 5;
        });

        options.AddPolicy(RateLimitingConstants.PerUserPolicy, context =>
        {
            var username = context.User?.Identity?.Name ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(username, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 50,
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 3
            });
        });

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 200,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            double? retryAfterSeconds = null;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                retryAfterSeconds = retryAfter.TotalSeconds;
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString();
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfterSeconds
            }, cancellationToken: cancellationToken);
        };
    });

    builder.Services.AddHealthChecks()
        .AddCheck(
            name: "self",
            check: () => HealthCheckResult.Healthy("Application is running"),
            tags: ["api"])
        .AddDbContextCheck<HotelListingDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql"]);

    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Hotel Listing API",
            Version = "v1"
        });

        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Title = "Hotel Listing API",
            Version = "v2"
        });

        options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        });

        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "API key required in the request header."
        });

        options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            Description = "Basic authentication."
        });

        options.ExampleFilters();
        options.OperationFilter<HotelListing.Api.Filters.SecurityRequirementsOperationFilter>();
    });

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

        options.GetLevel = (httpContext, elapsed, exception) =>
            exception != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode >= 400
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            }
        };
    });

    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Listing API V1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Hotel Listing API V2");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Hotel Listing API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseRateLimiter();

    app.UseAuthorization();

    app.UseOutputCache();

    app.MapHealthChecks("/healthz/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    });

    app.MapControllers();

    Log.Information("HotelListing API started successfully");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}