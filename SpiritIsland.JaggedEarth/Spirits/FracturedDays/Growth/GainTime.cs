namespace SpiritIsland.JaggedEarth;

public class GainTime : SpiritAction {

	readonly int _delta;

	public GainTime(int delta):base( $"GainTime({delta})" ) { _delta = delta; }

	public override async Task ActAsync( Spirit self ) {
		if( self is FracturedDaysSplitTheSky fracturedDays )
			await fracturedDays.GainTime(_delta);
	}

}