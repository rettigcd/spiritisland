using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class PhaseToImageSourceConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value is Phase phase) 
			value = phase.ToString();
		return value is string s ? s.ToLower() + ".png" 
			: (object?)null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Phase.None;
	}
}
