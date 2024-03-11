using System.Globalization;

namespace SpiritIsland.Maui;

public class HighlightIfMatchConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value is string propString && parameter is string paramString && propString == paramString
			? Colors.Yellow
			: (object)Colors.Transparent;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}

}
