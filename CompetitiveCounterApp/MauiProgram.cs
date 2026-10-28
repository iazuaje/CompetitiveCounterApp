using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace CompetitiveCounterApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS
    				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
#if ANDROID
                    handlers.AddHandler<Pages.Controls.ReorderScrollView, Pages.Controls.ReorderScrollViewHandler>();
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite(Constants.DatabasePath));

            builder.Services.AddSingleton<DatabaseInitializer>();

            builder.Services.AddSingleton<GameDataService>();
            builder.Services.AddTransient<GameOperationsService>();

            builder.Services.AddSingleton<GameRepository>();
            builder.Services.AddSingleton<PlayerRepository>();
            builder.Services.AddSingleton<SessionRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<ModalErrorHandler>();

            builder.Services.AddSingleton<GamesPageModel>();
            builder.Services.AddSingleton<PlayersPageModel>();

            builder.Services.AddTransientWithShellRoute<CreateGamePage, CreateGamePageModel>("creategame");
            builder.Services.AddTransientWithShellRoute<EditGamePage, EditGamePageModel>("editgame");
            builder.Services.AddTransientWithShellRoute<GameDetailPage, GameDetailPageModel>("gamedetail");
            builder.Services.AddTransientWithShellRoute<CreatePlayerPage, CreatePlayerPageModel>("createplayer");
            builder.Services.AddTransientWithShellRoute<EditPlayerPage, EditPlayerPageModel>("editplayer");
            builder.Services.AddTransientWithShellRoute<SessionDetailPage, SessionDetailPageModel>("sessiondetail");
            builder.Services.AddTransientWithShellRoute<GameStatsPage, GameStatsPageModel>("gamestats");
            // Raíz del flyout (ruta "stats" en AppShell.xaml): sin ruta apilada propia.
            builder.Services.AddTransient<GeneralStatsPageModel>();
            builder.Services.AddTransientWithShellRoute<PlayerStatsPage, PlayerStatsPageModel>("playerstats");
            builder.Services.AddTransientWithShellRoute<PlayerGameStatsPage, PlayerGameStatsPageModel>("playergamestats");

            // Campos dentro de una cápsula: sin la línea inferior nativa de Android.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
            {
#if ANDROID
                if (view is CompetitiveCounterApp.Pages.Controls.BorderlessEntry)
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            var app = builder.Build();

            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

            return app;
        }
    }
}
