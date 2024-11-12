using System.Globalization;

namespace SpiritIsland.Maui;

public class NameToBadgeImgConverter : IValueConverter {

	public object? Convert(object? value, Type _, object? parameter, CultureInfo _1) {
		return value is not string name ? null : (object)NameToImage(name);
	}

	public object? ConvertBack(object? value, Type _, object? _1, CultureInfo _2) {
		throw new NotImplementedException();
	}

	static public ImageSource NameToImage(string name) => ImageCache.FromFile($"bdg_{name.ToResourceName()}.png");
}
