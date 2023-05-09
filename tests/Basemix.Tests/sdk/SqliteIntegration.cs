using System.Data;
using Basemix.Db;
using Basemix.Lib;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats.Persistence;
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

public class SqliteFixture
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
    
    private void ProvisionDatabase()
    {
        var db = new Migrator(this.Database, NullLogger<Migrator>.Instance);
        db.Start();
    }
}
