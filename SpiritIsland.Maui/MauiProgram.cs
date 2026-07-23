using CommunityToolkit.Maui;

namespace SpiritIsland.Maui;

public static class MauiProgram {

	public static MauiApp CreateMauiApp() {

		var builder = MauiApp.CreateBuilder();
		builder
			.RegisterServices()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigurePlatform()
			.ConfigureFonts( fonts => {
#if WINDOWS
				// Unpackaged Windows apps can't resolve MauiFont resources via ms-appx:// at
				// runtime (see https://github.com/dotnet/maui/issues/26441), so load these
				// as plain embedded assembly resources instead.
				var assembly = typeof(MauiProgram).Assembly;
				fonts.AddEmbeddedResourceFont( assembly, "OpenSans-Regular.ttf", "OpenSansRegular" );
				fonts.AddEmbeddedResourceFont( assembly, "OpenSans-Semibold.ttf", "OpenSansSemibold" );
				fonts.AddEmbeddedResourceFont( assembly, "leaguegothic-regular-webfont.ttf", "LeagueGothicRegular" ); // game font
				fonts.AddEmbeddedResourceFont( assembly, "playsir-regular.otf", "PlaySirRegular" ); // invader cards
#else
				fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" );
				fonts.AddFont( "OpenSans-Semibold.ttf", "OpenSansSemibold" );
				fonts.AddFont( "leaguegothic-regular-webfont.ttf", "LeagueGothicRegular" ); // game font
				fonts.AddFont( "playsir-regular.otf", "PlaySirRegular" ); // invader cards
#endif
			} );

		//		builder.Services.AddTransientPopup<GrowthPopup, GrowthModel>();

#if DEBUG
		Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions.AddDebug(builder.Logging);
#endif

		return builder.Build();
	}

	public static MauiAppBuilder RegisterServices( this MauiAppBuilder app ) {
		app.Services.AddSingleton<NewGamePage>();
		app.Services.AddSingleton<SoloGamePage>();
		return app;
	}

}
