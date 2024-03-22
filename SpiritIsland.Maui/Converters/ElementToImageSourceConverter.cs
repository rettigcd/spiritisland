using System.Globalization;

namespace SpiritIsland.Maui;

public class ElementToImageSourceConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value is string s 
			? (s=="") ? null : Convert(ElementStrings.ParseEl(s)) 
			: value == null ? null : Convert((Element)value);
	}
	static object Convert(Element el) => el.GetTokenImg().ImgSource();

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Element.None;
	}

}

public class PhaseToImageSourceConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value is Phase phase) value = phase.ToString();
		return value is string s ? ImageCache.FromFile( s.ToLower()+".png" ) 
			: (object?)null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Phase.None;
	}
}