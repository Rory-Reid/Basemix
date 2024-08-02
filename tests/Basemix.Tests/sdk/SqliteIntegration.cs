using System.Data;
using Basemix.Db;
using Basemix.Lib;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats.Persistence;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basemix.Tests.sdk;

[Collection("sqlite-integration")]
public class SqliteIntegration
{
    private readonly SqliteFixture fixture;
    public SqliteIntegration(SqliteFixture fixture) => this.fixture = fixture;
}

[CollectionDefinition("sqlite-integration")]
public class SqliteCollection : ICollectionFixture<SqliteFixture>
{
}

public interface ISqliteFixture
{
    public SqliteRatsRepository RatsRepository { get; }
    public SqliteOwnersRepository OwnersRepository { get; }
}

public class SqliteFixture : ISqliteFixture, IAsyncLifetime
{
    public string Database => "test-db.sqlite";

    public IDbConnection GetConnection() => new SqliteConnection($"Data Source={this.Database};Pooling=false");

    public SqliteFixture()
    {
        if (File.Exists(this.Database))
        {
            File.Delete(this.Database);
        }

        this.ProvisionDatabase();
        DapperSetup.Configure();

        this.RatsRepository = new SqliteRatsRepository(this.GetConnection);
        this.OwnersRepository = new SqliteOwnersRepository(this.GetConnection);
    }

    public SqliteRatsRepository RatsRepository { get; }
    public SqliteOwnersRepository OwnersRepository { get; }
    
    public List<(long Id, string Reason)> DeathReasons { get; private set; } = new();
    
    private void ProvisionDatabase()
    {
        var db = new Migrator(this.Database, this.Database, NullLogger<Migrator>.Instance);
        db.Start();
    }

    public async Task InitializeAsync()
    {
        using var db = this.GetConnection();
        this.DeathReasons = (await db.QueryAsync<(long Id, string Reason)>(
            "SELECT id, reason FROM death_reason")).ToList();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
