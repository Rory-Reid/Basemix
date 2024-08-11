using Basemix.Db;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Basemix.Tests.Integration;

public class MigratorTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;

    public MigratorTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Can_run_migrator_twice_without_error()
    {
        // Already runs once as part of fixture, we just need to run it again
        var migrator = new Migrator(this.fixture.Database, this.fixture.Database, NullLogger<Migrator>.Instance);
        migrator.Start();
    }

    [Fact]
    public async Task Migrator_moves_db_from_legacy_path_to_new()
    {
        var dbName = this.faker.Random.AlphaNumeric(5);
        var legacyPath = $"{dbName}legacy.db";

        await using var db = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = legacyPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ConnectionString);
        db.Open();

        await db.ExecuteAsync("CREATE TABLE test (test_name TEXT PRIMARY KEY);");
        await db.ExecuteAsync("INSERT INTO test (test_name) VALUES (@TestName);",
            new {TestName = nameof(this.Migrator_moves_db_from_legacy_path_to_new)});
        db.Close();

        var newDbPath = $"{dbName}new.db";
        var migrator = new Migrator(newDbPath, legacyPath, NullLogger<Migrator>.Instance);
        migrator.Start();
        
        await using var newDb = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = newDbPath,
            Mode = SqliteOpenMode.ReadOnly
        }.ConnectionString);
        newDb.Open();
        
        var result = await newDb.QuerySingleAsync<string>("SELECT test_name FROM test;");
        result.ShouldBe(nameof(this.Migrator_moves_db_from_legacy_path_to_new));
    }
}