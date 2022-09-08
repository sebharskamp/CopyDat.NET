using CopyDat.Tests.Data.Models.BikeStore;
using CopyDat.Tests.Data.Models.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    public class BikeStoreDbContextFactory : IDesignTimeDbContextFactory<BikeStoresContext>
    {
        public BikeStoresContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BikeStoresContext>();
            optionsBuilder.UseSqlServer(
              "Server=.\\SQLExpress;Database=SourceDb.BikeStores.Testing;Trusted_Connection=True;");
            return new BikeStoresContext(optionsBuilder.Options);
        }
    }
}
