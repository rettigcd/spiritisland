namespace SpiritIsland.Basegame;

public class SwallowTheLandDwellers {

	public const string Name = "Swallow the Land-Dwellers";

	[SpiritCard(Name,0,Element.Water,Element.Earth),Slow,FromPresence(0,Target.Coastal)]
	[Instructions( "Drown 1 Explorer, 1 Town, and 1 Dahan." ), Artist( Artists.JoshuaWright )]
	static public async Task Act(TargetSpaceCtx ctx ) {

		// "Drown 1 explorer, 1 town, 1 dahan"

		// find Ocean's Hungry Grasp spirit
		var ocean = ctx.Self as Ocean ?? ctx.GameState.Spirits.Single(x=>x is Ocean);

		// find place to drown then
		var drowningOcean = ocean.Presence.Spaces.First() // find any space the ocean has presnece
			.Boards[0].Ocean; // find the Ocean space on that board

		// drown 1 explorer, 1 town, and 1 dahan

		// drown 1 explorer ( drop 1 explorer in the ocean to drown )
		var explorerToDrown = ctx.Tokens.OfHumanClass(Human.Explorer).OrderBy(x=>x.StrifeCount).FirstOrDefault();
		if(explorerToDrown != null)
			await ctx.MoveTo( explorerToDrown, drowningOcean );

		// drop town in the ocean to drown
		var townToDrown = ctx.Tokens.OfHumanClass(Human.Town)
			.OrderByDescending(x=>x.FullHealth) // items with most health - usually are all the same
			.ThenBy(x=>x.Damage) // pick least damaged
			.FirstOrDefault();
		if( townToDrown != null )
			await ctx.MoveTo( townToDrown, drowningOcean );

		await ctx.Dahan.Destroy(1); // destroying dahan is the same as drowning them
	}

}
