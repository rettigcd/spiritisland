namespace SpiritIsland.Basegame;

[InnatePower(Name), Slow]
[FromPresence(0,Filter.DahanAndIncarna)]
public class LeadTheWarriorsToBattle {
	public const string Name = "Lead the Warriors to Battle";

	[InnateTier("1 sun,2 fire", "1 Fear if Towns/Cities are present. 1 Damage.")]
	static public Task FearToTownsAndCitiesOption1Async(TargetSpaceCtx ctx) {
		// 1 Damage.
		return Inner(ctx,1);
	}

	[InnateTier("3 sun,3 fire", "1 Damage per Dahan.")]
	static public Task Option2Async(TargetSpaceCtx ctx) {
		//1 Damage per Dahan.
		return Inner(ctx, ctx.Space.Dahan.CountAll);
	}

	static async Task Inner(TargetSpaceCtx ctx, int damage) {
		// 1 Fear if Towns/Cities are present.
		if( ctx.Space.HasAny(Human.Town_City))
			await ctx.AddFear(1);
		await ctx.DamageInvaders(damage);
	}

}