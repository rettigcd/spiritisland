namespace SpiritIsland.BranchAndClaw;

public class FleshrotFever {

	[MinorCard( "Fleshrot Fever", 1, Element.Fire, Element.Air, Element.Water, Element.Animal )]
	[Slow]
	[FromPresence( 1, Target.JungleOrSand )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.Disease.Add(1);
	}

}