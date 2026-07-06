using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelListing.Api.Domain;

public class HotelListingDbContextFactory : IDesignTimeDbContextFactory<HotelListingDbContext>
{
    public HotelListingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelListingDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=HotelListingDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new HotelListingDbContext(optionsBuilder.Options);
    }
}