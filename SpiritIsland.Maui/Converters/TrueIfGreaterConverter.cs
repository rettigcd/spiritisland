using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class TrueIfGreaterConverter : IValueConverter {

	public object? Convert(object? value, Type _, object? parameter, CultureInfo _1) => ConvertToInt(parameter) < ConvertToInt(value);

	static int ConvertToInt(object? value) => value switch { string s => int.Parse(s), int i => i, _ => 0 };

	public object? ConvertBack(object? value, Type _, object? _1, CultureInfo _2) {
		throw new NotImplementedException();
	}

}
