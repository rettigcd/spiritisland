namespace SpiritIsland.Basegame;

public class SwallowTheLandDwellers {

	[SpiritCard("Swallow the Land-Dwellers",0,Element.Water,Element.Earth)]
	[Slow]
	[FromPresence(0,Target.Coastal)]
	static public async Task Act(TargetSpaceCtx ctx ) {

		// find Ocean's Hungry Grasp spirit
		var ocean = ctx.Self as Ocean ?? ctx.GameState.Spirits.Single(x=>x is Ocean);

		// find place to drown then
		var drowningOcean = ocean.Presence
			.Spaces.First() // find any space the ocean has presnece
			.Board.Ocean; // find the Ocean space on that board

		// drown 1 explorer, 1 town, and 1 dahan

		// drop explorer in the ocean to drown
		if( ctx.Tokens.Has(Invader.Explorer))
			await ctx.Move( Invader.Explorer[1], ctx.Space, drowningOcean );

		// drop town in the ocean to drown
		if(ctx.Invaders.Tokens.Has(Invader.Town))
			await ctx.Move( ctx.Invaders.Tokens[Invader.Town[2]]>0 ? Invader.Town[2] : Invader.Town[1], ctx.Space, drowningOcean );

		await ctx.DestroyDahan(1); // destroying dahan is the same as drowning them
	}

}
