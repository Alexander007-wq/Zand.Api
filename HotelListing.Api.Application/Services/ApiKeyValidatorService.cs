using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

// Validate ApiKeys against the dataase
public class ApiKeyValidatorService(HotelListingDbContext db) : IApiKeyValidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        var apiKeyEntity = await db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == apiKey, ct);
        if (apiKeyEntity == null) return false;

        // if there is no expiry date or the expiry date deoes not exceed today's date.
        return apiKeyEntity.IsActive;
    }
}
