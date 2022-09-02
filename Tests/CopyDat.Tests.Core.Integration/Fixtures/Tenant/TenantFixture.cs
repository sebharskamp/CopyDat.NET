using CopyDat.Tests.Core.Integration.Fixtures.BikeStore;
using CopyDat.Tests.Data.Models.Tenant;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    public class TenantFixture : IAsyncLifetime
    {
        private TenantContext? _context;

        public TenantContext GetContext()
        {
            if (_context is null) throw new InvalidOperationException("Something went wrong initializing the context. " +
                "Make sure to use collection attribute and inject this fixture on and into the test class.");
            return _context;
        }

        public async Task InitializeAsync()
        {
            var dbContextFactory = new TenantDbContextFactory();
            _context ??= dbContextFactory.CreateDbContext(new string[] { });
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
