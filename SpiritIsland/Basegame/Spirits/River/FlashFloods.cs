using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class FlashFloods {

		public const string Name = "Flash Floods";
		[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
		[FromPresence(1,Target.Invaders)]
		static public async Task ActionAsync(ActionEngine engine,Space target) {
			var (_, gameState) = engine;

			// +1 damage, if costal +1 additional damage
			int damage = target.IsCostal ? 2 : 1;

			await engine.UserSelectDamage( damage, gameState.InvadersOn( target ) );
		}

	}

}