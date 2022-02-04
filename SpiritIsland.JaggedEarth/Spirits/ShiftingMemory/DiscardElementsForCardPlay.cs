namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Shifting Memories Track-Action
/// </summary>
public class DiscardElementsForCardPlay : GrowthActionFactory, ITrackActionFactory {

	// When revealing Discard-Elements-For-Extra-Card-Plays during non-growth (i.e. Unrelenting Growth), don't activate - it is too late to add card plays.

	readonly int totalNumToRemove;
	public DiscardElementsForCardPlay(int elementDiscardCount ) {
		this.totalNumToRemove = elementDiscardCount;
	}

	public bool RunAfterGrowthResult => true; // delay for gained prepared elements.

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if( ctx.Self is ShiftingMemoryOfAges smoa
			&& totalNumToRemove <= smoa.PreparedElements.Total
			&& (await smoa.DiscardElements(totalNumToRemove,"additional card-play")).Count == totalNumToRemove
		) smoa.tempCardPlayBoost++;
	}

}