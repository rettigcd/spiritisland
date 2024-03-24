using System.Globalization;

namespace SpiritIsland.Maui;

public class PhaseToImageSourceConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		try {
			if(value is Phase phase) 
				value = phase.ToString();
			if( value is string s)
				return s.ToLower() + ".png";
		}
		catch (Exception ex) {
		}
		return (object?)null;

	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Phase.None;
	}
}