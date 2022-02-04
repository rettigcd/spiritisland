namespace SpiritIsland.JaggedEarth;

public class DissolvingVapors {

	[SpiritCard("Dissolving Vapors",2,Element.Air,Element.Water), Slow, FromPresence(0)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear
		ctx.AddFear(1);

		// 1 damage to each invader.
		await ctx.DamageEachInvader(1);

		// 1 damage to each dahan.
		await ctx.Apply1DamageToEachDahan();

	}

}