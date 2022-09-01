using Xunit;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    [CollectionDefinition(nameof(BikeStoreFixtureCollection))]
    public class BikeStoreFixtureCollection : ICollectionFixture<BikeStoreFixture>
    {
        // A class with no code, only used to define the collection
    }
}
