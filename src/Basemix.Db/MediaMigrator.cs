using System.Reflection;
using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Logging;

namespace Basemix.Db;

public class MediaMigrator(string databasePath, ILogger<MediaMigrator> logger)
{
    public void Start()
    {
        var upgrader =
            DeployChanges.To
                .SQLiteDatabase($"Data Source={databasePath};Pooling=false")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                    s => s.Contains(".Scripts.Media."))
                .WithVariablesDisabled()
                .LogTo(new LoggerAdapter(logger))
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError("Unable to perform media database upgrade, {Error}", result.Error);
            throw new Exception($"Failed to perform media database upgrade {result.Error}");
        }
    }

    private class LoggerAdapter(ILogger logger) : IUpgradeLog
    {
        public void WriteInformation(string format, params object[] args) => logger.LogInformation(format, args);
        public void WriteError(string format, params object[] args) => logger.LogError(format, args);
        public void WriteWarning(string format, params object[] args) => logger.LogWarning(format, args);
    }
}
