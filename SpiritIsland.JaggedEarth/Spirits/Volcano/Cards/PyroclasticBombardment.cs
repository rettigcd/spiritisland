namespace SpiritIsland.JaggedEarth;

public class PyroclasticBombardment {

	[SpiritCard("Pyroclastic Bombardment", 3, Element.Fire, Element.Air, Element.Earth), Fast, FromSacredSite(2)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 Damage to each town / city / dahan.
		await ctx.DamageEachInvader(1,Human.Town_City);
		await ctx.Apply1DamageToEachDahan();

		// 1 Damage
		await ctx.DamageInvaders( 1 );

		// 1 Damage to dahan.
		await ctx.DamageDahan( 1 );

	}

}