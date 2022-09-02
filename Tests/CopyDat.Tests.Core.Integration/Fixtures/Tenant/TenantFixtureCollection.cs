using Xunit;

namespace CopyDat.Tests.Core.Integration.Fixtures.Tenant
{
    [CollectionDefinition(nameof(TenantFixtureCollection))]
    public class TenantFixtureCollection : ICollectionFixture<TenantFixture>
    {
        // A class with no code, only used to define the collection
    }
}
