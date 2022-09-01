using CopyDat.Data.Models;
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
              "Server=.\\SQLExpress;Database=SourceDb.Testing;Trusted_Connection=True;");

            return new BikeStoresContext(optionsBuilder.Options, Seeder.GetBikeStore());
        }
    }
}
