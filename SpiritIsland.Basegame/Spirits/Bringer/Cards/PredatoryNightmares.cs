using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class PredatoryNightmares {
		
		// Predatory Nightmares => 2 => slow,
		// 1 from sacred site, invaders =>
		// moon, fire, mountain, animal =>
		[SpiritCard("Predatory Nightmares",2,Element.Moon,Element.Fire,Element.Earth,Element.Animal)]
		[Slow]
		[FromSacredSite(1,Target.Invaders)]
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
}
