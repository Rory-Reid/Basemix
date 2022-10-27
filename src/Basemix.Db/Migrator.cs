using System.Reflection;
using DbUp;
using DbUp.Engine.Output;
using DbUp.Helpers;
using Microsoft.Extensions.Logging;

namespace Basemix.Db;

public class Migrator
{
    private readonly string databaseName;
    private readonly ILogger<Migrator> logger;

    public Migrator(string databaseName, ILogger<Migrator> logger)
    {
        this.databaseName = databaseName;
        this.logger = logger;
    }

    public void Start()
    {
        var upgrader =
            DeployChanges.To
                .SQLiteDatabase($"Data Source={databaseName}")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithVariablesDisabled()
                .LogTo(new LoggerAdapter(this.logger))
                .JournalTo(new NullJournal())
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