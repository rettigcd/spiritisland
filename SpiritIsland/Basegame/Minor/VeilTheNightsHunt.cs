using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;

			int dahanCount = ctx.GameState.GetDahanOnSpace(target);
			string damageInvadersText = $"{dahanCount} damage to invaders";
			bool damageInvaders = ctx.GameState.HasInvaders(target)
				&& await ctx.Self.SelectText("Select card option", damageInvadersText, "push up to 3 dahan") == damageInvadersText;

			if(damageInvaders)
				// each dahan deals 1 damage to a different invader
				await ctx.DamageInvaders(target, dahanCount);
			else
				// push up to 3 dahan
				await ctx.PushUpToNDahan(target,3);
		}

	}
}
