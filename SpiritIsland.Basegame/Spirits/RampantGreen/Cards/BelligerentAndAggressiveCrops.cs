namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

// 1 Damage, to Towns/Cities only. If there are any adjacent Wilds: 1 Fear. 1 Damage, to Towns/Cities only.

public class BelligerentAndAggressiveCrops {

	public const string Name = "Belligerent and Aggressive Crops";

	[SpiritCard(Name, 1, "sun, fire, plant"), Slow, FromSacredSite(2, [Filter.Town, Filter.City])]
	[Instructions("1 Damage, to Towns/Cities only. Add 1 Wilds. If there are any adjacent Wilds: 1 Fear. 1 Damage, to Towns/Cities only."), Artist(Artists.DavidMarkiwsky)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// Add 1 wilds
		await ctx.Wilds.AddAsync(1);
		// 1 Damage, to Towns/Cities only.
		await ctx.DamageInvaders(1,Human.Town_City);

		// If there are any adjacent Wilds:
		if( ctx.Space.Adjacent.Any(x => x.Wilds.Any) ) {
			// 1 Fear.
			await ctx.AddFear(1);
			// 1 Damage, to Towns/Cities only.
			await ctx.DamageInvaders(1, Human.Town_City);
		}

	}

}