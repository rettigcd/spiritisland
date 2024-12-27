namespace SpiritIsland.Horizons;

public class MarkTerritoryWithScarsAndTeeth {

	public const string Name = "Mark Territory with Scars and Teeth";

	[SpiritCard(Name,2,Element.Sun,Element.Earth,Element.Animal),Fast,FromPresence(0)]
	[Instructions( "Defend 9.  2 Fear if Invaders are present. Push 2 Dahan." ), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Defend 9.
		ctx.Defend(9);
		// 2 Fear if Invaders are present.
		if(ctx.HasInvaders)
			await ctx.AddFear(2);
		// Push 2 Dahan.
		await ctx.PushDahan(2);
	}

}
