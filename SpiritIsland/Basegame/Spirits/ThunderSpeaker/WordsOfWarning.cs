using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class WordsOfWarning {


		[SpiritCard( "Words of Warning", 1, Speed.Fast, Element.Air, Element.Sun, Element.Animal )]
		[FromPresence(1,Filter.Dahan)]
		static public Task Act( ActionEngine engine, Space target ) {
			// defend 3.
			engine.GameState.Defend(target,3);

			// During ravage, dahan in target land deal damange simultaneously with invaders

			// !!!! Give each invader an attack phase - Invaders 0, Dahan 1
				// calculate everyone in the same phase at the same time

			return Task.CompletedTask;
		}


	}
}
