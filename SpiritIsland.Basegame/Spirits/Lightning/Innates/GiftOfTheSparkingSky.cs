namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast, AnySpirit]
public class GiftOfTheSparkingSky {

	public const string Name = "Gift of the Sparking Sky";

	[InnateTier("2 sun,5 fire,3 air", "Once this turn, after target Spirit uses a Power that targets a land, they may do 1 Damage to each Invader in that land.")]
	public static Task Destroy_Town(TargetSpiritCtx ctx) {
		// Once this turn, after target Spirit uses a Power that targets a land, they may do 1 Damage to each Invader in that land.
		// (This is their Action and gets all benefits and penalties that apply to their Powers.)
		RunSpaceActionOnceOnFutureTarget.Trigger(ctx.Other, new SpaceAction("Apply 1 Damage to Each Invader", ctx => ctx.DamageEachInvader(1)));
		return Task.CompletedTask;
	}

	[InnateTier("2 fire, 2 air", "Target Spirit gains a Minor Power.")]
	public static Task Destroy_TownOrCity(TargetSpiritCtx ctx) {
		// Target Spirit gains a Minor Power.
		return ctx.Other.Draw.Minor(1);
	}

	// 1 Sun 3 Fire 2 Air — 1 Sun, 3 Fire, 2 Air
	[InnateTier("1 sun, 3 fire,2 air", "Target Spirit may play a Power Card by paying its cost.")]
	public static Task Destroy_2TownsOrCities(TargetSpiritCtx ctx) {
		// Target Spirit may play a Power Card by paying its cost.
		// (If you target yourself, its Elements arrive too late to apply to prior thresholds of this Power.)
		return ctx.Other.SelectAndPlayCardsFromHand(1);
	}

}
