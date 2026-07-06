using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Api.Domain;

public class ApiKey
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(200)]
    public string AppName { get; set; } = string.Empty;

    public DateTimeOffset? ExpiresAtUth { get; set; }

    public DateTimeOffset CreatedAtUth { get; set; } = DateTimeOffset.UtcNow;

    [NotMapped]
    public bool IsActive => !ExpiresAtUth.HasValue || ExpiresAtUth.Value > DateTimeOffset.UtcNow;
}
