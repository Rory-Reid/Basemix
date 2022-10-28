// using Basemix.Db;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Serilog;
// using Serilog.Debugging;
// using Serilog.Formatting.Json;
//
// SelfLog.Enable(Console.Out);
//
// var config = new ConfigurationBuilder()
//     .AddJsonFile("appsettings.json", optional:true, reloadOnChange:false)
//     .AddEnvironmentVariables()
//     .Build();
//     
// var logger = new LoggerConfiguration()
//     .MinimumLevel.Debug()
//     .Enrich.FromLogContext()
//     .Enrich.WithProperty("app", "migrator")
//     .WriteTo.Console(new JsonFormatter(renderMessage: true))
//     .CreateLogger();
//
// var services = new ServiceCollection();
// services.AddLogging(builder => builder.AddSerilog(logger));
// services.AddSingleton<Migrator>();
//
// var provider = services.BuildServiceProvider();
//
// // Run it
// var bootstrapLogger = provider.GetRequiredService<ILogger<Program>>();
// bootstrapLogger.LogInformation("Running manual migration");
// var migrator = provider.GetRequiredService<Migrator>();
//
// migrator.Start();