namespace SpiritIsland.JaggedEarth;

[InnatePower("Fire Burns, Water Soothes"), Slow, FromSacredSite(1)]
class FireBurnsWaterSoothes {

	[InnateTier("3 fire","1 Fear. 2 Damage.")]
	static public async Task Option1(TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.DamageInvaders(2);
	}

	[InnateTier("3 water","Remove 1 blight.",1)]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.RemoveBlight();
	}

}