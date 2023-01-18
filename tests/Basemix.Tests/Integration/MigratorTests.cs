using Basemix.Db;
using Basemix.Tests.sdk;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basemix.Tests.Integration;

public class MigratorTests : SqliteIntegration
{
    private readonly SqliteFixture fixture;

    public MigratorTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Can_run_migrator_twice_without_error()
    {
        // Already runs once as part of fixture, we just need to run it again
        var migrator = new Migrator(this.fixture.Database, NullLogger<Migrator>.Instance);
        migrator.Start();
    }
}