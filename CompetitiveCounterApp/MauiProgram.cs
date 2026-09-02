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
#if IOS || MACCATALYST
    				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
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
            builder.Services.AddSingleton<ModalErrorHandler>();

            builder.Services.AddSingleton<GamesPageModel>();
            builder.Services.AddSingleton<PlayersPageModel>();

            builder.Services.AddTransientWithShellRoute<CreateGamePage, CreateGamePageModel>("creategame");
            builder.Services.AddTransientWithShellRoute<EditGamePage, EditGamePageModel>("editgame");
            builder.Services.AddTransientWithShellRoute<GameDetailPage, GameDetailPageModel>("gamedetail");
            builder.Services.AddTransientWithShellRoute<CreatePlayerPage, CreatePlayerPageModel>("createplayer");
            builder.Services.AddTransientWithShellRoute<EditPlayerPage, EditPlayerPageModel>("editplayer");
            builder.Services.AddTransientWithShellRoute<SessionDetailPage, SessionDetailPageModel>("sessiondetail");

            var app = builder.Build();

            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

            return app;
        }
    }
}
