using System.Globalization;

namespace SpiritIsland.Maui;

public class OptionStateToColorConverter : IValueConverter {

	public object? Convert(object? value, Type _, object? _1, CultureInfo _2) {
		return value is OptionState os ? OptionStateToColor(os) : Colors.Transparent;
	}

	public object? ConvertBack(object? value, Type _, object? _1, CultureInfo _2) {
		throw new NotImplementedException();
	}

	static public Color OptionStateToColor( OptionState state ) => state switch { 
		OptionState.Selected => Colors.Green, 
		OptionState.IsOption => Colors.Red, 
		_ => Colors.Transparent
	};
}
