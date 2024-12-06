using System.Collections;
using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class NotEmptyConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value switch {
			IList list => 0 < list.Count,
			int i => i != 0,
			string s => !string.IsNullOrWhiteSpace(s),
			_ => false
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}