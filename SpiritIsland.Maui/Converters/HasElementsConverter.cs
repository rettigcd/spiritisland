using System.Globalization;

namespace SpiritIsland.Maui;

public class HasElementsConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {

		return value is ECouldHaveElements eche
			? eche switch {
				ECouldHaveElements.Yes => Colors.DarkSalmon,
				ECouldHaveElements.AsPrepared => Colors.Wheat,
				_ => Colors.Transparent
			}
			: (object)Colors.Transparent;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return ECouldHaveElements.No;
	}
}