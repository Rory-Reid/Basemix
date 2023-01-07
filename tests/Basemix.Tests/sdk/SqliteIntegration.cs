using System.Data;
using System.Runtime.CompilerServices;
using Basemix.Db;
using Basemix.Rats.Persistence;
using Bogus;
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

    public IDbConnection GetConnection() => new SqliteConnection($"Data Source={this.Database}");

    public SqliteFixture()
    {
        if (File.Exists(this.Database))
        {
            File.Delete(this.Database);
        }

        this.ProvisionDatabase();
        DapperSetup.Configure();

        this.RatsRepository = new SqliteRatsRepository(this.GetConnection);
    }

    public SqliteRatsRepository RatsRepository { get; }
    
    private void ProvisionDatabase()
    {
        var db = new Migrator(this.Database, NullLogger<Migrator>.Instance);
        db.Start();
    }
}
