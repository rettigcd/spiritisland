namespace SpiritIsland.Basegame;

[InnatePower(Name), Slow]
[FromSacredSite(1,Filter.Invaders)]
public class LightningTornSkiesIncitePandemonium {

	public const string Name = "Lightning-Torn Skies Incite Pandemonium";

	// 3 Fire 2 Air
	[InnateTier("3 fire, 2 air", "2 Fear. Add 1 Strife.")]
	public static Task FearAnd1Strife(TargetSpaceCtx ctx) {
		// 2 Fear. Add 1 Strife.
		return FearAndStrife(ctx,1);
	}

	// 4 Fire 3 Air
	[InnateTier("4 fire, 3 air", "2 Fear. Add 1 Strife.")]
	public static Task FearAnd2Strife(TargetSpaceCtx ctx) {
		// 2 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 2);
	}

	// 5 Fire 4 Air 1 Moon
	[InnateTier("5 fire,4 air,1 moon", "3 Fear. Add 1 Strife.")]
	public static Task FearAnd3Strife(TargetSpaceCtx ctx) {
		// 3 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 3);
	}

	// 5 Fire 5 Air 2 Moon
	[InnateTier("5 fire, 5 air, 2 moon", "4 Fear. Add 1 Strife.")]
	public static Task FearAnd4Strife(TargetSpaceCtx ctx) {
		// 4 Fear. Add 1 Strife.
		return FearAndStrife(ctx, 4);
	}

	static async Task FearAndStrife(TargetSpaceCtx ctx, int fearCount, int strifeCount=1) {
		await ctx.AddFear(fearCount);
		await ctx.AddStrife(strifeCount);
	}

}