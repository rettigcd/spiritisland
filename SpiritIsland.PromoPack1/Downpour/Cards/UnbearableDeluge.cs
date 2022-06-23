namespace SpiritIsland.PromoPack1;

internal class UnbearableDeluge {

	const string Name = "Unbearable Deluge";

	[SpiritCard( Name, 0, Element.Air, Element.Water, Element.Earth )]
	[Fast]
	[FromPresence(0)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 1 Fear.
		ctx.AddFear(1);

		// Push 2 Dahan.
		await ctx.PushDahan(2);

		// Defend 3
		ctx.Defend(3);

		// If target land is wetland, isolate it
		if(ctx.IsOneOf(Terrain.Wetland))
			ctx.Isolate();
	}

}