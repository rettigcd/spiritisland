namespace SpiritIsland.Basegame;

[InnatePower(Name), Slow]
[FromSacredSite(1,Filter.Invaders)]
public class LightningTornSkiesIncitePandemonium {

	public const string Name = "Lightning-Torn Skies Incite Pandemonium";

	// 3 Fire 2 Air
	[InnateTier("3 fire, 2 air", "2 Fear. Add 1 Strife.")]
	public static Task Destroy_Town(TargetSpaceCtx ctx) {
		// 2 Fear. Add 1 Strife.
		return FearAndStrife(ctx,2);
	}

	// 4 Fire 3 Air
	[InnateTier("4 fire, 3 air", "2 Fear. Add 1 Strife.")]
	public static Task Destroy_TownOrCity(TargetSpaceCtx ctx) {
		// 2 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 2);
	}

	// 5 Fire 4 Air 1 Moon
	[InnateTier("5 fire,4 air,1 moon", "3 Fear. Add 1 Strife.")]
	public static Task Destroy_2TownsOrCities(TargetSpaceCtx ctx) {
		// 3 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 3);
	}

	// 5 Fire 5 Air 2 Moon
	[InnateTier("5 fire, 5 air, 2 moon", "4 Fear. Add 1 Strife.")]
	public static Task Destroy_3TownsOrCities(TargetSpaceCtx ctx) {
		// 4 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 4);
	}

	static async Task FearAndStrife(TargetSpaceCtx ctx, int fearCount, int strifeCount=1) {
		await ctx.AddFear(fearCount);
		await ctx.AddStrife(strifeCount);
	}

}