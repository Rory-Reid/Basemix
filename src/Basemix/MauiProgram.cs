using Basemix.Db;
using Basemix.Lib;
using Basemix.Lib.Ingestion.RatRecordsSpreadsheet;
using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats.Persistence;
using Basemix.Lib.Settings.Persistence;
using Basemix.Pages;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace Basemix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); })
                .UseMauiCommunityToolkit();

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            builder.Services.AddBasemix();

            return builder.Build();
        }

        public static void AddBasemix(this IServiceCollection services)
        {
            var errorContext = new ErrorContext();
            DapperSetup.Configure();
            Parser.Configure();
            try
            {
                PdfGenerator.RegisterFonts();
            }
            catch (Exception e)
            {
                errorContext.LastError = e.ToString();
            }

            var docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var basemixPath = Path.Combine(docsDirectory, "basemix");
            Directory.CreateDirectory(basemixPath);
            var dbPath = Path.Combine(basemixPath, "db.sqlite");

            services.AddSingleton<GetDataDirectory>(() => basemixPath);
            services.AddSingleton<GetDatabasePath>(() => dbPath);
            services.AddSingleton(s => new Migrator(dbPath, s.GetRequiredService<ILogger<Migrator>>()));
            services.AddSingleton<GetDatabase>(() => new SqliteConnection($"Data Source={dbPath};Pooling=false"));
            services.AddSingleton<NowDateOnly>(() => DateOnly.FromDateTime(DateTime.Now));
            services.AddSingleton<DateSpanToString>(Delegates.HumaniseDateSpan);
            services.AddSingleton<BreedersRepository>();
            services.AddSingleton<IRatsRepository, SqliteRatsRepository>();
            services.AddSingleton<ILittersRepository, SqliteLittersRepository>();
            services.AddSingleton<IPedigreeRepository, SqlitePedigreeRepository>();
            services.AddSingleton<IOwnersRepository, SqliteOwnersRepository>();
            services.AddSingleton<IProfileRepository, SqliteProfileRepository>();
            services.AddSingleton<PdfGenerator>();
            services.AddSingleton(errorContext);
            services.AddSingleton<LitterEstimator>();
            services.AddSingleton<LitterEstimator.GetEstimationParameters>(() => EstimationParameters.Standard);

            // UI Nonsense
            services.AddSingleton<JsInteropExports>();
            services.AddSingleton<HistoryBack>(s => s.GetRequiredService<JsInteropExports>().HistoryBack);
        }
    }
}