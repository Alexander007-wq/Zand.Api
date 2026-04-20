using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Zand.Api.Data
{
    public class ZandDbContext : DbContext
    {
        public ZandDbContext(DbContextOptions<ZandDbContext> options) : base(options)
        {
            
        }
    }
}
