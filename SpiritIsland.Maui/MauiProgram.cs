using CommunityToolkit.Maui;

namespace SpiritIsland.Maui; 
public static class MauiProgram {
	public static MauiApp CreateMauiApp() {
		var builder = MauiApp.CreateBuilder();
		builder
			.RegisterServices()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts( fonts => {
				fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" );
				fonts.AddFont( "OpenSans-Semibold.ttf", "OpenSansSemibold" );
				fonts.AddFont( "leaguegothic-regular-webfont.ttf", "LeagueGothicRegular" ); // game font
				fonts.AddFont( "playsir-regular.otf", "PlaySirRegular" ); // invader cards
			} );

		//		builder.Services.AddTransientPopup<GrowthPopup, GrowthModel>();

#if DEBUG
		Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions.AddDebug(builder.Logging);
#endif

		return builder.Build();
	}

	public static MauiAppBuilder RegisterServices( this MauiAppBuilder app ) {
//		app.Services.AddSingleton<Models.GrowthModel>();
		app.Services.AddSingleton<NewGamePage>();
		app.Services.AddSingleton<SinglePlayerGamePage>();
		return app;
	}

}
