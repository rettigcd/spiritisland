namespace SpiritIsland.Basegame;

public class SwallowTheLandDwellers {

	[SpiritCard("Swallow the Land-Dwellers",0,Element.Water,Element.Earth)]
	[Slow]
	[FromPresence(0,Target.Coastal)]
	static public async Task Act(TargetSpaceCtx ctx ) {

		// "Drown 1 explorer, 1 town, 1 dahan"

		// find Ocean's Hungry Grasp spirit
		var ocean = ctx.Self as Ocean ?? ctx.GameState.Spirits.Single(x=>x is Ocean);

		// find place to drown then
		var drowningOcean = ocean.Presence
			.Spaces( ctx.GameState ).First() // find any space the ocean has presnece
			.Board.Ocean; // find the Ocean space on that board

		// drown 1 explorer, 1 town, and 1 dahan

		// drown 1 explorer ( drop 1 explorer in the ocean to drown )
		var explorerToDrown = ctx.Tokens.OfType(Invader.Explorer).Cast<HealthToken>().OrderBy(x=>x.StrifeCount).FirstOrDefault();
		if(explorerToDrown != null)
			await ctx.Move( explorerToDrown, ctx.Space, drowningOcean );

		// drop town in the ocean to drown
		var townToDrown = ctx.Tokens.OfType(Invader.Town).Cast<HealthToken>()
			.OrderByDescending(x=>x.FullHealth) // items with most health - usually are all the same
			.ThenBy(x=>x.Damage) // pick least damaged
			.FirstOrDefault();
		if( townToDrown != null )
			await ctx.Move( townToDrown, ctx.Space, drowningOcean );

		await ctx.DestroyDahan(1); // destroying dahan is the same as drowning them
	}

}
