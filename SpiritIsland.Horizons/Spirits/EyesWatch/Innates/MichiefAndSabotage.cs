namespace SpiritIsland.Horizons;

[InnatePower(Name)]
[Fast,FromSacredSite(1,Filter.Invaders)]
public class MichiefAndSabotage {

	public const string Name = "Michief and Sabotage";

	[InnateTier("1 moon,2 plant","1 Fear. Defend 2",0)]
	static public Task Option1(TargetSpaceCtx ctx) {
		return FearAndDefend(ctx,1,2);
	}

	[InnateTier("2 moon,3 plant","Instead, 1 Fear. Defend 4",0)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return FearAndDefend(ctx, 1, 4);
	}

	[InnateTier("2 moon,2 air,4 plant", "Instead, 3 Fear. Defend 6", 0)]
	static public Task Option3(TargetSpaceCtx ctx) {
		return FearAndDefend(ctx, 3, 6);
	}

	[InnateTier("3 moon,3 air,5 plant", "Instead, 5 Fear. Defend 12", 0)]
	static public Task Option4(TargetSpaceCtx ctx) {
		return FearAndDefend(ctx, 5, 12);
	}

	static Task FearAndDefend(TargetSpaceCtx ctx, int fear, int defend) {
		ctx.Defend(defend);
		var bob = ctx.Space.ToString();
		return ctx.AddFear(fear);
	}
}
