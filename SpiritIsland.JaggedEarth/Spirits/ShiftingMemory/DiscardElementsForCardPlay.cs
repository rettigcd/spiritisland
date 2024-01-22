namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Shifting Memories Track-Action
/// </summary>
public class DiscardElementsForCardPlay( int elementDiscardCount ) 
	: SpiritAction( "DiscardElementsForCardPlay" ) 
{

	// When revealing Discard-Elements-For-Extra-Card-Plays during non-growth (i.e. Unrelenting Growth), don't activate - it is too late to add card plays.

	readonly int _totalNumToRemove = elementDiscardCount;

	public override async Task ActAsync( Spirit self ) {
		if( self is ShiftingMemoryOfAges smoa
			&& _totalNumToRemove <= smoa.PreparedElements.Total
			&& (await smoa.DiscardElements(_totalNumToRemove,"additional card-play")).Count == _totalNumToRemove
		) smoa.TempCardPlayBoost++;
	}

}