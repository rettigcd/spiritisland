namespace SpiritIsland.Horizons;

public class FoulVaporsAndFetidMuck {

	public const string Name = "Foul Vapors and Fetid Muck";

	[SpiritCard(Name, 0, Element.Fire, Element.Air, Element.Water, Element.Earth), Slow, FromSacredSite(2,Filter.Invaders)]
	[Instructions("1 Fear. Push up to 2 Explorer"), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 1 Fear.
		await ctx.AddFear(1);
		// Push up to 2 Explorer
		await ctx.PushUpTo(2,Human.Explorer);
	}

}
