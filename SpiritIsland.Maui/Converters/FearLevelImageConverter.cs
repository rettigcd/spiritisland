using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class FearLevelImageConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is string s)
			value = string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
		if( value is not int i ) return null;

		if( i == 0 ) return ImageCache.FromFile("disease.png");

		var result = ImageCache.FromFile($"tl{i}.png");
		return result;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return 1;
	}

}
