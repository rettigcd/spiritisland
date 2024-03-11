using System.Globalization;

namespace SpiritIsland.Maui;

public class ElementToImageSourceConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value is string s) {
			if(string.IsNullOrEmpty(s)) return null;
			Element el = ElementStrings.ParseEl(s);
			return el.GetTokenImg().ImgSource();
		}
		return value == null ? null : (object)((Element)value).GetTokenImg().ImgSource();
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Element.None;
	}

}
