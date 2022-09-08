using CopyDat.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using FluentAssertions;
using CopyDat.Tests.Data.Models.Tenant;
using EFCore.BulkExtensions;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    [Collection(nameof(TenantFixtureCollection))]
    public class TenantFixtureTestsCopyWithFilter
    {
        private readonly TenantFixture _fixture;

        public TenantFixtureTestsCopyWithFilter(TenantFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CopyDatabaseByTenant()
        {
            var context = _fixture.GetContext();
            var targetOptionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            targetOptionsBuilder.UseSqlServer(
              $"Server=.\\SQLExpress;Database=TargetDb.Tenant.{nameof(CopyDatabaseByTenant)}.Testing;Trusted_Connection=True;");

            var targetContext = await targetOptionsBuilder.InitiateConnectionAsync(true);

            await targetContext.Database.EnsureDeletedAsync();
            await targetContext.Database.EnsureCreatedAsync();

            var filterByTenant = new TenantFilter(1);

            var subcriptionEntityFilter = new SubscriptionEntityFilter(filterByTenant,
                t => t.BaseType == typeof(TenantEntity));

            var resourceGroupEntityFilter = new ResourceGroupEntityFilter(subcriptionEntityFilter,
                t => t.BaseType == typeof(SubscriptionEntity));

            var copyDat = await CopyDat<TenantContext>.InitializeAsync(context, targetContext);
            await copyDat.CopyAsync(new System.Collections.Generic.List<CopyDat.Core.FilterStrategies.IFilterStrategy> { filterByTenant, subcriptionEntityFilter, resourceGroupEntityFilter });

            await context.EntitiesShouldBeEqual(targetContext, filterByTenant, options => options.Excluding(t => t.Subscriptions));
            await context.EntitiesShouldBeEqual<TenantContext, Subscription>(targetContext, subcriptionEntityFilter, options => {
                options.Excluding(s => s.ResourceGroups);
                options.Excluding(s => s.Tenant);
                return options;
            });
            await context.EntitiesShouldBeEqual<TenantContext, ResourceGroup>(targetContext, resourceGroupEntityFilter, options => {
                options.Excluding(r => r.Subscription);
                return options;
            });
        }
    }
}


