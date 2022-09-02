using CopyDat.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using FluentAssertions;
using CopyDat.Tests.Data.Models.Tenant;
using CopyDat.Tests.Core.Integration.Fixtures.Tenant;

namespace CopyDat.Tests.Core.Integration
{
    [Collection(nameof(TenantFixtureCollection))]
    public class CopyDatTenantTests
    {
        private readonly TenantFixture _fixture;

        public CopyDatTenantTests(TenantFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CopyDatabaseByTenant()
        {
            var context = _fixture.GetContext();
            var targetOptionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            targetOptionsBuilder.UseSqlServer(
              "Server=.\\SQLExpress;Database=TargetDb.Testing;Trusted_Connection=True;");
            var targetContext = await targetOptionsBuilder.InitiateConnectionAsync(true);

            var filterByTenant = new TenantFilter(1);
            var subcriptionEntityFilter = new SubscriptionEntityFilter();
            subcriptionEntityFilter.SetPrerequisite(filterByTenant);

            var resourceGroupEntityFilter = new ResourceGroupEntityFilter();
            resourceGroupEntityFilter.SetPrerequisite(subcriptionEntityFilter);

            var copyDat = await CopyDat<TenantContext>.InitializeAsync(context, targetContext);
            await copyDat.CopyAsync(new System.Collections.Generic.List<CopyDat.Core.FilterStrategies.IFilterStrategy> { filterByTenant, subcriptionEntityFilter });

            var products = await context.ResourceGroups.ToListAsync();
            var targetProducts = await targetContext.ResourceGroups.ToListAsync();
            Assert.NotEmpty(products);
            targetProducts.Should().BeEquivalentTo(products, options =>
            {
                options.Excluding(p => p.Subscription);
                return options;
            });
        }
    }
}


