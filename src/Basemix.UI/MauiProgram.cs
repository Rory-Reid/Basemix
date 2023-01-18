﻿using Basemix.Db;
using Basemix.Litters.Persistence;
using Basemix.Rats.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Basemix.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            builder.Services.AddBasemix();
            
            return builder.Build();
        }

        public static void AddBasemix(this IServiceCollection services)
        {
            DapperSetup.Configure();

            var docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var basemixPath = Path.Combine(docsDirectory, "basemix");
            Directory.CreateDirectory(basemixPath);
            var dbPath = Path.Combine(basemixPath, "db.sqlite");

            services.AddSingleton<GetDatabasePath>(() => dbPath);
            services.AddSingleton(s => new Migrator(dbPath, s.GetRequiredService<ILogger<Migrator>>()));
            services.AddSingleton<GetDatabase>(() => new SqliteConnection($"Data Source={dbPath}"));
            services.AddSingleton<BreedersRepository>();
            services.AddSingleton<IRatsRepository, SqliteRatsRepository>();
            services.AddSingleton<ILittersRepository, SqliteLittersRepository>();
            
            // UI Nonsense
            services.AddSingleton<JsInteropExports>();
            services.AddSingleton<HistoryBack>(s => s.GetRequiredService<JsInteropExports>().HistoryBack);
        }
    }
}