namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Shifting Memories Track-Action
/// </summary>
public class DiscardElementsForCardPlay : GrowthActionFactory, IActionFactory {

	// When revealing Discard-Elements-For-Extra-Card-Plays during non-growth (i.e. Unrelenting Growth), don't activate - it is too late to add card plays.

	readonly int totalNumToRemove;
	public DiscardElementsForCardPlay(int elementDiscardCount ) {
		this.totalNumToRemove = elementDiscardCount;
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if( ctx.Self is ShiftingMemoryOfAges smoa
			&& totalNumToRemove <= smoa.PreparedElements.Total
			&& (await smoa.DiscardElements(totalNumToRemove,"additional card-play")).Count == totalNumToRemove
		) smoa.tempCardPlayBoost++;
	}

}