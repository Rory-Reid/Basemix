using Basemix.Db;
using Microsoft.Extensions.Logging;

namespace Basemix.UI;

public partial class App : Application
{
    private readonly ILoggerFactory loggerFactory;

    public App(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        InitializeComponent();

        this.SetupDb();
        MainPage = new AppShell();
    }

    private void SetupDb()
    {
        var migrator = new Migrator("0000.db", this.loggerFactory.CreateLogger<Migrator>());
        migrator.Start();
    }
}