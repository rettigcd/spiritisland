namespace SpiritIsland.BranchAndClaw;

public class FleshrotFever {

	[MinorCard( "Fleshrot Fever", 1, Element.Fire, Element.Air, Element.Water, Element.Animal ),Slow,FromPresence( 1, Target.Jungle, Target.Sand )]
	[Instructions( "1 Fear. Add 1 Disease." ),Artist(Artists.JoshuaWright)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.Disease.AddAsync(1);
	}

}