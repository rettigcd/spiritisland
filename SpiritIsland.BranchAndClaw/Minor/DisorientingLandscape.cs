namespace SpiritIsland.BranchAndClaw;

public class DisorientingLandscape {

	[MinorCard( "Disorienting Landscape", 1, Element.Moon, Element.Air, Element.Plant )]
	[Fast]
	[FromSacredSite( 2 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await ctx.Push(1, Invader.Explorer);

		if(ctx.Space.IsOneOf(Terrain.Mountain,Terrain.Jungle))
			await ctx.Wilds.Add(1);
	}

}