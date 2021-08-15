using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class FavorsCalledDue {

		[SpiritCard("Favors Called Due",1,Speed.Slow,Element.Moon,Element.Air,Element.Animal)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine engine, Space target){
			var (_,gameState) = engine;

			// gather up to 4 dahan
			await engine.GatherUpToNDahan( target, 4 );

			// if invaders are present and dahan now out numberthem, 3 fear
			var invaderCount = engine.GameState.InvadersOn(target).TotalCount;
			if(invaderCount > 0 && gameState.GetDahanOnSpace( target ) > invaderCount) {
				gameState.AddFear( 3 );
			}

		}
	}

}
