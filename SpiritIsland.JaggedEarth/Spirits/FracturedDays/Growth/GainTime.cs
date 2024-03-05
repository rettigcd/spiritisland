namespace SpiritIsland.JaggedEarth;

public class GainTime( int _delta ) 
	: SpiritAction( $"Gain Time({_delta})" ) 
{
	public override async Task ActAsync( Spirit self ) {
		if( self is FracturedDaysSplitTheSky fracturedDays )
			await fracturedDays.GainTime(_delta);
	}
}