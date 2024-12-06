using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class ElementToImageSourceConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value is string s 
			? (s=="") ? null : Convert(ElementStrings.ParseEl(s)) 
			: value == null ? null : Convert((Element)value);
	}
	static ImageSource Convert(Element el) => el.GetTokenImg().ImgSource();

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return Element.None;
	}

}
