using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using System.Collections;
using CopyDat.Tests.Data.Models.BikeStore;
using CopyDat.Tests.Data.Models.Tenant;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    [Collection(nameof(TenantFixtureCollection))]
    public class TenantFixtureSeed
    {
        private readonly TenantFixture _fixture;

        public TenantFixtureSeed(TenantFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(typeof(Subscription))]
        [InlineData(typeof(ResourceGroup))]
        [InlineData(typeof(Data.Models.Tenant.Tenant))]
        public async Task HasEntity_WhenHasEnsuredToBeCreated(Type type)
        {
            var context = _fixture.GetContext();
            var assertNotEmpty = GetType().GetMethod(nameof(TenantFixtureSeed.AsserNotEmptyCollection), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (assertNotEmpty is null) throw new MethodAccessException($"{nameof(TenantFixtureSeed.AsserNotEmptyCollection)} not found.");
            assertNotEmpty = assertNotEmpty.MakeGenericMethod(new[] { type });
            await assertNotEmpty.InvokeAsync(this, new[] { context });
        }

        private async Task AsserNotEmptyCollection<T>(TenantContext context) where T : class
        {
            var entities = await context.Set<T>().ToListAsync();
            Assert.NotEmpty(entities);
        }
    }
}
