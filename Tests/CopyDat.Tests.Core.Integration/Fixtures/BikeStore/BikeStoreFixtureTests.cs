using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using System.Collections;
using CopyDat.Tests.Data.Models.BikeStore;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    [Collection(nameof(BikeStoreFixtureCollection))]
    public class BikeStoreFixtureSeed
    {
        private readonly BikeStoreFixture _fixture;

        public BikeStoreFixtureSeed(BikeStoreFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(typeof(Brands))]
        [InlineData(typeof(Categories))]
        [InlineData(typeof(Customers))]
        [InlineData(typeof(OrderItems))]
        [InlineData(typeof(Orders))]
        [InlineData(typeof(Products))]
        [InlineData(typeof(Staffs))]
        [InlineData(typeof(Stocks))]
        [InlineData(typeof(Stores))]
        public async Task HasEntity_WhenHasEnsuredToBeCreated(Type type)
        {
            var context = _fixture.GetContext();
            var assertNotEmpty = GetType().GetMethod(nameof(BikeStoreFixtureSeed.AsserNotEmptyCollection), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (assertNotEmpty is null) throw new MethodAccessException($"{nameof(BikeStoreFixtureSeed.AsserNotEmptyCollection)} not found.");
            assertNotEmpty = assertNotEmpty.MakeGenericMethod(new[] { type });
            await assertNotEmpty.InvokeAsync(this, new[] { context });
        }

        private async Task AsserNotEmptyCollection<T>(BikeStoresContext context) where T : class
        {
            var entities = await context.Set<T>().ToListAsync();
            Assert.NotEmpty(entities);
        }
    }
}
