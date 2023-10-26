namespace SpiritIsland.NatureIncarnate;

public class DrawToTheWatersEdge {

	[SpiritCard( "Draw to the Water's Edge", 1, Element.Sun, Element.Water, Element.Plant ),Fast,FromPresence( 0 )]
	[Instructions( "Gather up to 2 Town from a single land." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// "Gather up to 2 Town from a single land."
		Space? source = null;
		var gatherer = ctx.Gatherer
			.AddGroup(2, Human.Town)
			.FilterSource(spaceState => source is null || spaceState.Space == source);
		gatherer.Collected += (st) => source ??= st.Space;
		await gatherer.GatherUpToN();
	}

}
