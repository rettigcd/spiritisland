namespace SpiritIsland.Basegame;

public class SwallowTheLandDwellers {

	public const string Name = "Swallow the Land-Dwellers";

	[SpiritCard(Name,0,Element.Water,Element.Earth),Slow,FromPresence(0,Filter.Coastal)]
	[Instructions( "Drown 1 Explorer, 1 Town, and 1 Dahan." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Drown 1 explorer, 1 town, and 1 dahan
		var drowner = Drowning.GetDrowner();

		// drown 1 explorer
		HumanToken explorerToDrown = ctx.Space.HumanOfTag(Human.Explorer).OrderBy(x => x.StrifeCount).FirstOrDefault();
		if( explorerToDrown != null )
			await drowner.Drown(explorerToDrown.On(ctx.Space));

		// Drown town
		HumanToken townToDrown = PickTownToDrown(ctx);
		if( townToDrown != null )
			await drowner.Drown(townToDrown.On(ctx.Space));

		await ctx.Dahan.Destroy(1); // destroying dahan is the same as drowning them
	}

	static HumanToken PickTownToDrown(TargetSpaceCtx ctx) {
		return ctx.Space.HumanOfTag(Human.Town)
			.OrderByDescending(x => x.FullHealth) // items with most health - usually are all the same
			.ThenBy(x => x.Damage) // pick least damaged
			.FirstOrDefault();
	}
}