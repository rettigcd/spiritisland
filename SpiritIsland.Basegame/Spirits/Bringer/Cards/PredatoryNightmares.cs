namespace SpiritIsland.Basegame;

public class PredatoryNightmares {
		
	[SpiritCard("Predatory Nightmares",2,Element.Moon,Element.Fire,Element.Earth,Element.Animal),Slow,FromSacredSite(1,Filter.Invaders)]
	[Instructions("2 Damage. Push up to 2 Dahan. (When your powers would destroy Invaders, instead they generate Fear and/or Push those Invaders.)"),Artist( Artists.ShaneTyree)] 
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 damange.
		await ctx.DamageInvaders(2);
		// Push up to 2 dahan.
		await ctx.PushUpToNDahan( 2 );

		// When your Powers would destroy invaders, instead they generate fear and/or push those invaders
		// NO! - Bringer gets this by default
		// If this card is traded to another spirit, it is too hard to swap out their InvaderGroup builder
	}

}
