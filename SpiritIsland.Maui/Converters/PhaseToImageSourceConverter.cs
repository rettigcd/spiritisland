using System.Globalization;

namespace SpiritIsland.Maui;

public class PhaseToImageSourceConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		try {
			if(value is Phase phase) 
				value = phase.ToString();
			if( value is string s) {
				if(s == "Slow")
					_lastWasSlow = true;
				else if( _lastWasSlow) {
				}
				return ImageCache.FromFile( s.ToLower()+".png" );
			}
		} catch(Exception ex) {
		}
		return (object?)null;

	}
	static bool _lastWasSlow;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Phase.None;
	}
}