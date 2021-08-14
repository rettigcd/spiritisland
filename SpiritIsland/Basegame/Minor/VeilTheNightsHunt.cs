using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public async Task Act( ActionEngine engine, Space target ) {

			int dahanCount = engine.GameState.GetDahanOnSpace(target);
			string damageInvadersText = $"{dahanCount} damage to invaders";
			bool damageInvaders = engine.GameState.HasInvaders(target)
				&& await engine.SelectText("Select card option", damageInvadersText, "push up to 3 dahan") == damageInvadersText;

			if(damageInvaders)
				// each dahan deals 1 damage to a different invader
				engine.GameState.DamageInvaders(target,dahanCount);
			else
				// push up to 3 dahan
				await engine.PushUpToNDahan(target,3);
		}

	}
}
