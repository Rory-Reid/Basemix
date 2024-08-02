using System.Reflection;
using Basemix.Lib.Persistence;
using DbUp;
using DbUp.Engine.Output;
using DbUp.Helpers;
using Microsoft.Extensions.Logging;

namespace Basemix.Db;

public class Migrator(string databaseName, string legacyDatabaseName, ILogger<Migrator> logger)
{
    private readonly string databaseName = databaseName;
    private readonly string legacyDatabaseName = legacyDatabaseName;
    private readonly ILogger<Migrator> logger = logger;

    public void Start()
    {
        // // Move db from old directories - thanks to a net8 breaking change
        // // See https://github.com/dotnet/runtime/issues/102110
        // // See https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix
        // // TODO - this but better
        // var legacyDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // var legacyDbPath = Path.Combine(legacyDirectory, "basemix/db.sqlite");
        //
        // var docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        // var basemixPath = Path.Combine(docsDirectory, "basemix");
        // Directory.CreateDirectory(basemixPath);
        // var dbPath = Path.Combine(basemixPath, "db.sqlite");
        //
        // if (File.Exists(legacyDbPath) && !File.Exists(dbPath))
        // {
        //     File.Move(legacyDbPath, dbPath);
        // }
        
        BasemixData.TryMoveDb(this.legacyDatabaseName, this.databaseName);
        
        // Run sql migrations
        var upgrader =
            DeployChanges.To
                .SQLiteDatabase($"Data Source={this.databaseName};Pooling=false")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithVariablesDisabled()
                .LogTo(new LoggerAdapter(this.logger))
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            this.logger.LogError("Unable to perform database upgrade, {Error}", result.Error);
            throw new Exception($"Failed to perform database upgrade {result.Error}");
        }
    }

    private class LoggerAdapter : IUpgradeLog
    {
        private readonly ILogger logger;
        public LoggerAdapter(ILogger logger) => this.logger = logger;
        public void WriteInformation(string format, params object[] args) => this.logger.LogInformation(format, args);
        public void WriteError(string format, params object[] args) => this.logger.LogError(format, args);
        public void WriteWarning(string format, params object[] args) => this.logger.LogWarning(format, args);
    }
}