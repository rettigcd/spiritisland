namespace SpiritIsland.Tests;

internal class SpiritConfigContext {
	readonly Spirit _spirit;

	public SpiritConfigContext(Spirit spirit) {
		_spirit = spirit;
	}

	public SpiritConfigContext Elements( string elementString ) {
		var counts = ElementStrings.Parse( elementString );
		foreach(var el in counts.Keys)
			_spirit.Elements[el] = counts[el];
		return this;
	}

}

