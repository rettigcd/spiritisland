using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class NameToBadgeImgConverter : IValueConverter {

	public object? Convert(object? value, Type _, object? parameter, CultureInfo _1) {
		return value is not string name ? null : (object)NameToImage(name);
	}

	public object? ConvertBack(object? value, Type _, object? _1, CultureInfo _2) {
		throw new NotImplementedException();
	}

	static public ImageSource NameToImage(string name) {
		string filename = $"bdg_{name.ToResourceName()}.png";
#if WINDOWS
		// Unpackaged Windows apps can't resolve MauiImage resources via ms-appx:// at runtime,
		// so badge images are also embedded as plain assembly resources - read those instead.
		return ImageSource.FromStream(() => typeof(NameToBadgeImgConverter).Assembly.GetManifestResourceStream(filename)!);
#else
		return ImageCache.FromFile(filename);
#endif
	}
}
