namespace SpiritIsland.JaggedEarth;

public class DissolvingVapors {

	[SpiritCard("Dissolving Vapors",2,Element.Air,Element.Water), Slow, FromPresence(0)]
	[Instructions( "1 Fear. 1 Damage to each Invader. 1 Damage to each Dahan." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear
		await ctx.AddFear(1);

		// 1 damage to each invader.
		await ctx.DamageEachInvader(1);

		// 1 damage to each dahan.
		await ctx.Apply1DamageToEachDahan();

	}

}