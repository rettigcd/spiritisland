namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast]
[FromPresence(2)]
public class StretchOutCoilsOfForebodingDread {

	static public void InitAspect(Spirit spirit){
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(StretchOutCoilsOfForebodingDread))];
	}

	public const string Name = "Stretch Out Coils of Foreboding Dread";

	[InnateTier("2 air", "Your other Powers may ignore Range when targeting the target land.")]
	public static Task Gather1Explorer(TargetSpaceCtx ctx) {
		// Your other Powers may ignore Range when targeting the target land.
		ctx.Self.PowerRangeCalc = new IncludeALandRangeCalculator(ctx.Self, ctx.Self.PowerRangeCalc, ctx.Space);
		return Task.CompletedTask;
	}

	[InnateTier("1 moon", "After an Action generates Fear in target land, including from Destroying Towns/Cities: Push up to 1 Explorer per Fear / 1 Town per 2 Fear.")]
	public static Task Plus_Destroy2Explorers(TargetSpaceCtx ctx) {
		// After an Action generates Fear in target land, (including from Destroying Towns/Cities):
		// Push up to 1 Explorer per Fear / 1 Town per 2 Fear. (You may mix-and-match.)
		ctx.Space.Init(new FearPushesInvaders(),1);
		return Task.CompletedTask;
	}

	[InnateTier("2 fire", "1 fear")]
	public static Task Plus_3Damage(TargetSpaceCtx ctx) {
		// 1 Fear.
		return ctx.AddFear(1);
	}

	[InnateTier("2 moon,4 air", "2 Fear.")]
	public static Task Plus_3Damag1222e(TargetSpaceCtx ctx) {
		// 2 Fear.
		return ctx.AddFear(2);
	}


}
