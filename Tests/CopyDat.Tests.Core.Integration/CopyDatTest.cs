using CopyDat.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using FluentAssertions;
using CopyDat.Tests.Core.Integration.Fixtures.BikeStore;
using System.Linq;
using CopyDat.Tests.Data.Models.BikeStore;

namespace CopyDat.Tests.Core.Integration
{
    [Collection(nameof(BikeStoreFixtureCollection))]
    public class CopyDatTest
    {
        private readonly BikeStoreFixture _fixture;

        public CopyDatTest(BikeStoreFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CopyDatabase()
        {
            var context = _fixture.GetContext();
            var targetOptionsBuilder = new DbContextOptionsBuilder<BikeStoresContext>();
            targetOptionsBuilder.UseSqlServer(
              "Server=.\\SQLExpress;Database=TargetDb.Testing;Trusted_Connection=True;");
            var targetContext = await targetOptionsBuilder.InitiateConnectionAsync(true);

            var copyDat = await CopyDat<BikeStoresContext>.InitializeAsync(context, targetContext);
            await copyDat.CopyAsync();

            var products = await context.Products.ToListAsync();
            var targetProducts = await targetContext.Products.ToListAsync();
            Assert.NotEmpty(products);
            targetProducts.Should().BeEquivalentTo(products, options =>
            {
                options.Excluding(p => p.Category);
                options.Excluding(p => p.Brand);
                options.Excluding(p => p.OrderItems);
                options.Excluding(p => p.Stocks);
                return options;
            });
            var stocks = await context.Stocks.ToListAsync();
            Assert.NotEmpty(stocks);
            var targetStocks = await targetContext.Stocks.ToListAsync();
            targetStocks.Should().BeEquivalentTo(stocks, options =>
            {
                options.Excluding(s => s.Product);
                options.Excluding(s => s.Store);
                return options;
            });
        }
    }
}


