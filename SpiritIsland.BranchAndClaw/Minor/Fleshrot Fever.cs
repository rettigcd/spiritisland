namespace SpiritIsland.BranchAndClaw;

public class FleshrotFever {

	[MinorCard( "Fleshrot Fever", 1, Element.Fire, Element.Air, Element.Water, Element.Animal ),Slow,FromPresence( 1, Filter.Jungle, Filter.Sands )]
	[Instructions( "1 Fear. Add 1 Disease." ),Artist(Artists.JoshuaWright)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.Disease.AddAsync(1);
	}

}