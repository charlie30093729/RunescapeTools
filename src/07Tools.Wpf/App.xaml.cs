using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.DependencyInjection;
using RunescapeTools.Infrastructure.Persistence;
using RunescapeTools.Wpf.Services;
using RunescapeTools.Wpf.ViewModels;

namespace RunescapeTools.Wpf;

public partial class App : System.Windows.Application
{
    private readonly CancellationTokenSource shutdown = new();
    private IHost? host;
    private Mutex? singleInstanceMutex;
    private bool ownsMutex;
    private Mutex? legacyInstanceMutex;
    private bool ownsLegacyMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        singleInstanceMutex = new Mutex(true, @"Local\07Tools.Desktop", out ownsMutex);
        if (!ownsMutex)
        {
            Shutdown();
            return;
        }

        // Also exclude an older executable while importing its saved state.
        legacyInstanceMutex = new Mutex(true, @"Local\RunescapeTools.Desktop", out ownsLegacyMutex);
        if (!ownsLegacyMutex)
        {
            Shutdown();
            return;
        }

        try
        {
            host = BuildHost();
            await host.Services.GetRequiredService<DesktopDataMigrator>().MigrateAsync(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), shutdown.Token);
            await host.StartAsync(shutdown.Token);

            var window = host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();

            await host.Services.GetRequiredService<ShellViewModel>().InitializeAsync(shutdown.Token);
            _ = WarmFavouritesAsync(host.Services, shutdown.Token);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"07Tools could not start.\n\n{exception.Message}",
                "07Tools",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        shutdown.Cancel();

        if (host is not null)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await host.StopAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                // The process is already exiting; do not block on a slow service shutdown.
            }

            host.Dispose();
        }

        if (ownsLegacyMutex)
            legacyInstanceMutex?.ReleaseMutex();
        legacyInstanceMutex?.Dispose();
        if (ownsMutex)
            singleInstanceMutex?.ReleaseMutex();
        singleInstanceMutex?.Dispose();
        shutdown.Dispose();

        base.OnExit(e);
    }

    private static IHost BuildHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localData = Path.Combine(localAppData, DesktopDataMigrator.ProductDirectoryName);

        builder.Services.AddRunescapeToolsServices(
            new OsrsWikiOptions(),
            new FavouriteStoreOptions
            {
                FilePath = Path.Combine(localData, "data", "favourites.json"),
                SeedJson = ReadEmbeddedSeed()
            },
            new MarketDataOptions(),
            trainingPlanOptions: new TrainingPlanOptions
            {
                FilePath = Path.Combine(localData, "data", "training-plans.json")
            },
            moneyMakingPreferenceOptions: new MoneyMakingPreferenceOptions
            {
                FilePath = Path.Combine(localData, "data", "money-making-preferences.json")
            },
            itemIconCacheOptions: new ItemIconCacheOptions
            {
                DirectoryPath = Path.Combine(localData, "data", "item-icons")
            },
            priceHistoryStoreOptions: new PriceHistoryStoreOptions(
                Path.Combine(localData, "data", "price-history")));

        builder.Services.AddSingleton(new ProfilePreferenceOptions
        {
            FilePath = Path.Combine(localData, "data", "profile.json"),
            DefaultRsn = "bottleo"
        });
        builder.Services.AddSingleton<IProfilePreferenceStore, JsonProfilePreferenceStore>();
        builder.Services.AddSingleton<ICurrentProfileContext, CurrentProfileContext>();

        builder.Services.AddSingleton<ProfileViewModel>();
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<FavouritesViewModel>();
        builder.Services.AddSingleton<MoneyMakerSelectionContext>();
        builder.Services.AddSingleton<MoneyMakersViewModel>();
        builder.Services.AddSingleton<
            ITrainingConfigurationDialogService,
            TrainingConfigurationDialogService>();
        builder.Services.AddSingleton<
            ITrainingPriceDialogService,
            TrainingPriceDialogService>();
        builder.Services.AddSingleton<XpPlannerViewModel>();
        builder.Services.AddSingleton<ShellViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        return builder.Build();
    }

    private static string ReadEmbeddedSeed()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("SeedFavourites.json")
                           ?? throw new InvalidOperationException("The first-run favourites snapshot is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task WarmFavouritesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILogger<App>>();
        try
        {
            await services.GetRequiredService<IFavouriteHistoryWarmupService>().WarmAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Favourite history warmup did not complete.");
        }
    }
}
