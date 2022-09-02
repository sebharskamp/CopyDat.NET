using CopyDat.Tests.Core.Integration.Fixtures.BikeStore;
using CopyDat.Tests.Data.Models.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantContext>
    {
        public TenantContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            optionsBuilder.UseSqlServer(
              "Server=.\\SQLExpress;Database=SourceDb.Testing;Trusted_Connection=True;");

            return new TenantContext(optionsBuilder.Options, Seeder.GetTenant());
        }
    }
}
