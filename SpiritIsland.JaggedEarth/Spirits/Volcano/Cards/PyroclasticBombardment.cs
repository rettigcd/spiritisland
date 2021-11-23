using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PyroclasticBombardment {
		[SpiritCard("Pyroclastic Bombardment", 3, Element.Fire, Element.Air, Element.Earth), Fast, FromSacredSite(2)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// 1 Damage to each town / city / dahan.
			await ctx.DamageEachInvader(1,Invader.Town, Invader.City);
			await ctx.Apply1DamageToAllDahan();

			// 1 Damage
			await ctx.Invaders.UserSelectedDamage( 1, ctx.Self ); // skipping over badlands

			// 1 Damage to dahan.
			await ctx.Dahan.ApplyDamage(1, ctx.Cause ); // skipping over badlands

		}

	}

}
