namespace SpiritIsland.Tests;

internal class SpiritConfigContext( Spirit spirit ) {
	readonly Spirit _spirit = spirit;

	public SpiritConfigContext Elements( string elementString ) {
		var counts = ElementStrings.Parse( elementString );
		foreach(var el in counts.Keys)
			_spirit.Elements[el] = counts[el];
		return this;
	}

}

