using System.Data;
using Basemix.Db;
using Basemix.Lib;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats.Persistence;
using Basemix.Tests.sdk;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basemix.Tests.Integration.SqliteStatisticsRepositoryTests;

[Collection("stats-sqlite-integration")]
public class StatsSqliteIntegration : IAsyncLifetime
{
    private readonly StatsSqliteFixture fixture;
    public StatsSqliteIntegration(StatsSqliteFixture fixture) => this.fixture = fixture;

    public Task InitializeAsync() => this.fixture.TruncateDb();
    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("stats-sqlite-integration")]
public class StatsSqliteCollection : ICollectionFixture<StatsSqliteFixture>
{
}

public class StatsSqliteFixture : ISqliteFixture
{
    public string Database => "stats-db.sqlite";

    public IDbConnection GetConnection() => new SqliteConnection($"Data Source={this.Database};Pooling=false");

    public StatsSqliteFixture()
    {
        this.DeleteDatabase();
        this.ProvisionDatabase();
        DapperSetup.Configure();
        
        this.RatsRepository = new SqliteRatsRepository(this.GetConnection);
        this.OwnersRepository = new SqliteOwnersRepository(this.GetConnection);
        this.LittersRepository = new SqliteLittersRepository(this.GetConnection);
    }

    public async Task TruncateDb()
    {
        using var db = this.GetConnection();
        await db.ExecuteAsync(
            """
            DELETE FROM rat;
            DELETE FROM litter;
            DELETE FROM owner;
            """);
    }
    
    private void ProvisionDatabase()
    {
        var db = new Migrator(this.Database, NullLogger<Migrator>.Instance);
        db.Start();
    }

    private void DeleteDatabase()
    {
        if (File.Exists(this.Database))
        {
            File.Delete(this.Database);
        }
    }

    public SqliteRatsRepository RatsRepository { get; }
    public SqliteOwnersRepository OwnersRepository { get; }
    public SqliteLittersRepository LittersRepository { get; }
}
