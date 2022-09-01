using CopyDat.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CopyDat.Core.Extensions;
using System.Collections;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    [Collection(nameof(BikeStoreFixtureCollection))]
    public class BikeStoreFixtureTests
    {
        private readonly BikeStoreFixture _fixture;

        public BikeStoreFixtureTests(BikeStoreFixture fixture)
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
            var toListAsync = GetType().GetMethod(nameof(BikeStoreFixtureTests.AsserNotEmptyCollection), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            toListAsync = toListAsync.MakeGenericMethod(new[] { type });
            await toListAsync.InvokeAsync(this, new[] { context });
        }

        private async Task AsserNotEmptyCollection<T>(BikeStoresContext context) where T : class
        {
            var entities = await context.Set<T>().ToListAsync();
            Assert.NotEmpty(entities);
        }
    }
}
