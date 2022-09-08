using CopyDat.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using FluentAssertions;
using CopyDat.Tests.Data.Models.Tenant;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    [Collection(nameof(TenantFixtureCollection))]
    public class TenantFixtureTestsSimpleCopy
    {
        private readonly TenantFixture _fixture;

        public TenantFixtureTestsSimpleCopy(TenantFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task StraigthForwardCopy()
        {
            var context = _fixture.GetContext();
            var targetOptionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            targetOptionsBuilder.UseSqlServer(
               $"Server=.\\SQLExpress;Database=TargetDb.Tenant.{nameof(StraigthForwardCopy)}.Testing;Trusted_Connection=True;");
            var targetContext = await targetOptionsBuilder.InitiateConnectionAsync(true);

            await targetContext.Database.EnsureDeletedAsync();
            await targetContext.Database.EnsureCreatedAsync();

            var copyDat = await CopyDat<TenantContext>.InitializeAsync(context, targetContext);
            await copyDat.CopyAsync();

            await context.EntitiesShouldBeEqual<TenantContext, Data.Models.Tenant.Tenant>(targetContext, options => options.Excluding(t => t.Subscriptions));
            await context.EntitiesShouldBeEqual<TenantContext, Subscription>(targetContext, options => {
                options.Excluding(s => s.ResourceGroups);
                options.Excluding(s => s.Tenant);
                return options;
            });
            await context.EntitiesShouldBeEqual<TenantContext, ResourceGroup>(targetContext, options => {
                options.Excluding(r => r.Subscription);
                return options;
            });
        }
    }
}


