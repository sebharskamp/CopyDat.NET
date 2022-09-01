using CopyDat.Data.Models;
using System.Threading.Tasks;
using Xunit;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    public class BikeStoreFixture : IAsyncLifetime
    {
        private BikeStoresContext _context;

        public BikeStoresContext GetContext()
        {
            return _context;
        }

        public async Task InitializeAsync()
        {
            var dbContextFactory = new BikeStoreDbContextFactory();
            _context = dbContextFactory.CreateDbContext(new string[] { });
            await _context.Database.EnsureCreatedAsync();
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
