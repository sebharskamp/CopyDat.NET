using CopyDat.Tests.Data.Models.BikeStore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    public class BikeStoreFixture : IAsyncLifetime
    {
        private BikeStoresContext? _context;

        public BikeStoresContext GetContext()
        {
            if (_context is null) throw new InvalidOperationException("Something went wrong initializing the context. " +
                "Make sure to use collection attribute and inject this fixture on and into the test class.");
            return _context;
        }

        public async Task InitializeAsync()
        {
            var dbContextFactory = new BikeStoreDbContextFactory();
            _context ??= dbContextFactory.CreateDbContext(new string[] { });
            await _context.Database.EnsureCreatedAsync();
            await _context.PopulateAsync(Seeder.GetBikeStore());
        }

        public async Task DisposeAsync()
        {
            if (_context != null)
            {
                await _context.Database.EnsureDeletedAsync();
                _context.Dispose();
            }
        }
    }
}
