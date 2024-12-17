namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast, AnotherSpirit]
[RepeatIf("5 fire, 2 water")]
public class ExaltationOfTheStormWind {

	public const string Name = "Exaltation of the Storm-Wind";

	[InnateTier("1 air", "This turn, you and target Spirit may each make one of your non-Major Slow Powers Fast.")]
	public static Task RunSlowAsFast(TargetSpiritCtx ctx) {
		// This turn, you and target Spirit may each make one of your non-Major Slow Powers Fast.
		ctx.Self.Mods.Add(new Run1SlowNonMajorAsFast(ctx.Self));
		ctx.Other.Mods.Add(new Run1SlowNonMajorAsFast(ctx.Other));
		return Task.CompletedTask;
	}

	[InnateTier("3 air", "You and target Spirit each gain +1 Range with all your Powers this turn.")]
	public static Task RangePlusOne(TargetSpiritCtx ctx) {
		// You and target Spirit each gain +1 Range with all your Powers this turn.
		RangeExtender.Extend( ctx.Self, 1 );
		RangeExtender.Extend( ctx.Other, 1 );
		return Task.CompletedTask;
	}

	[InnateTier("4 air,1 water", "You and target Spirit may each Push up to 2 Explorers from one of your respective lands.")]
	public static async Task Push2Explorers(TargetSpiritCtx ctx) {
		// You and target Spirit may each Push up to 2 Explorers from one of your respective lands.
		var cmd = Cmd.PushUpToNExplorers(2).From().SpiritPickedLand().Which(Has.YourPresence);
		await cmd.ActAsync(ctx.Self);
		await cmd.ActAsync(ctx.Other);
	}

}

