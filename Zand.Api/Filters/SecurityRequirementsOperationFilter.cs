using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelListing.Api.Filters;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerAttributes = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true) ?? [];

        var methodAttributes = context.MethodInfo.GetCustomAttributes(true);

        var allAttributes = controllerAttributes
            .Union(methodAttributes)
            .ToList();

        var hasAuthorize = allAttributes.OfType<AuthorizeAttribute>().Any();
        var hasAllowAnonymous = methodAttributes.OfType<AllowAnonymousAttribute>().Any();

        if (!hasAuthorize || hasAllowAnonymous)
        {
            return;
        }

        operation.Responses ??= [];

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized"
        });

        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden"
        });

        var securityRequirements = new List<OpenApiSecurityRequirement>();

        var hasApiKeyAuth = allAttributes.Any(attr =>
            attr.GetType().Name.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));

        var hasBasicAuth = allAttributes.Any(attr =>
            attr.GetType().Name.Contains("Basic", StringComparison.OrdinalIgnoreCase));

        if (hasApiKeyAuth)
        {
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("ApiKey", context.Document)] = []
            });
        }

        if (hasBasicAuth)
        {
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Basic", context.Document)] = []
            });
        }

        if (securityRequirements.Count == 0)
        {
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", context.Document)] = []
            });
        }
        {
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", context.Document)] = []
            });
        }

        operation.Security = securityRequirements;
    }
}