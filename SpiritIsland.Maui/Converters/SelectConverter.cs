using System.Globalization;

namespace SpiritIsland.Maui;

[AcceptEmptyServiceProvider]
public class SelectConverter : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value is CardUse cu ? cu.ToString().Equals(parameter)
			: value is Overlay overlay && overlay.ToString().Equals(parameter);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}

}
