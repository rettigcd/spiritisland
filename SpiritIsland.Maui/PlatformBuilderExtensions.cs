namespace SpiritIsland.Maui;

static public partial class PlatformBuilderExtensions {
	static public MauiAppBuilder ConfigurePlatform( this MauiAppBuilder builder) {
		Configure(builder);
		return builder;
	}
	static private partial void Configure( MauiAppBuilder builder);

#if TEST_STUB
	static private partial void Configure(MauiAppBuilder builder) {}
#endif

}