using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class ShatterHomesteads {
		public const string Name = "Shatter Homesteads";

		[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
		[FromSacredSite(2)]
		static public Task Act(ActionEngine engine, Space target){
			var (_,gameState) = engine;
			// Destroy 1 town
			gameState.InvadersOn(target).Destroy( Invader.Town, 1 );

			// 1 fear
			gameState.AddFear(1);
			return Task.CompletedTask;
		}

	}

}
