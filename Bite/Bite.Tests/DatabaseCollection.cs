using Xunit;

namespace Bite.Tests
{
    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // Diese Klasse hat keinen Code.
        // Sie dient xUnit nur als Marker, damit DatabaseFixture
        // einmal für alle [Collection("Database")]-Klassen geteilt wird.
    }
}