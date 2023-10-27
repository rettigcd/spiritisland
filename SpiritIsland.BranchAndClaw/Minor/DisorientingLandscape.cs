namespace SpiritIsland.BranchAndClaw;

public class DisorientingLandscape {

	[MinorCard( "Disorienting Landscape", 1, Element.Moon, Element.Air, Element.Plant ),Fast,FromSacredSite( 2 )]
	[Instructions( "Push 1 Explorer. If target land is Mountain / Jungle, add 1 Wilds." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await ctx.Push(1, Human.Explorer);

		if(ctx.Space.IsOneOf(Terrain.Mountain,Terrain.Jungle))
			await ctx.Wilds.AddAsync(1);
	}

}