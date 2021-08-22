using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public async Task Act( TargetSpaceCtx ctx ) {

			bool damageInvaders = ctx.HasInvaders
				&& await ctx.Self.UserSelectsFirstText( "Select card option", $"{ctx.DahanCount} damage to invaders", "push up to 3 dahan" );

			if(damageInvaders)
				// each dahan deals 1 damage to a different invader
				await ctx.DamageInvaders( ctx.DahanCount );
			else
				// push up to 3 dahan
				await ctx.PowerPushUpToNDahan(3);
		}

	}
}
