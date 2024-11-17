using SpiritIsland.Maui.Platforms.Android;

namespace SpiritIsland.Maui;

static public partial class PlatformBuilderExtensions {
	static private partial void Configure(MauiAppBuilder builder) {
		builder.Services.AddSingleton<INotificationManagerService>(NotificationManagerService.Instance);
	}
}