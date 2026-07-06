using HotelListing.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Domain.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "22a749f6-99cb-4254-91b7-b68319f3cc3f",
                Name = RoleName.Administrator,
                NormalizedName = RoleName.Administrator.ToUpper(),
                ConcurrencyStamp = "22a749f6-99cb-4254-91b7-b68319f3cc3f"
            },
            new IdentityRole
            {
                Id = "8f6da150-60c9-4069-ad71-bf2815825eb7",
                Name = RoleName.User,
                NormalizedName = RoleName.User.ToUpper(),
                ConcurrencyStamp = "8f6da150-60c9-4069-ad71-bf2815825eb7"
            },
            
            new IdentityRole
            {
                Id = "a7715e6f-6eee-4064-99f2-b8b56975c5db",
                Name = RoleName.HotelAdmin,
                NormalizedName = RoleName.HotelAdmin.ToUpper(),
                ConcurrencyStamp = "a7715e6f-6eee-4064-99f2-b8b56975c5db"
            }
            );

    }
}
