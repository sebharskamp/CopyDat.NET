using CopyDat.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using FluentAssertions;
using System.Linq;
using CopyDat.Tests.Data.Models.BikeStore;
using CopyDat.Tests.Core.Integration;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    [Collection(nameof(BikeStoreFixtureCollection))]
    public class BikeStoreFixtureTestsSimpleCopy
    {
        private readonly BikeStoreFixture _fixture;

        public BikeStoreFixtureTestsSimpleCopy(BikeStoreFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task StraigthForwardCopy()
        {
            var context = _fixture.GetContext();
            var targetOptionsBuilder = new DbContextOptionsBuilder<BikeStoresContext>();
            targetOptionsBuilder.UseSqlServer(
              $"Server=.\\SQLExpress;Database=TargetDb.BikeStores.{nameof(StraigthForwardCopy)}.Testing;Trusted_Connection=True;");
            var targetContext = await targetOptionsBuilder.InitiateConnectionAsync(true);

            await targetContext.Database.EnsureDeletedAsync();
            await targetContext.Database.EnsureCreatedAsync();

            var copyDat = await CopyDat<BikeStoresContext>.InitializeAsync(context, targetContext);
            await copyDat.CopyAsync();

            await context.EntitiesShouldBeEqual<BikeStoresContext, Brands>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Categories>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Customers>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, OrderItems>( targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Orders>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Products>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Staffs>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Stocks>(targetContext);
            await context.EntitiesShouldBeEqual<BikeStoresContext, Stores>(targetContext);

        }
    }
}


