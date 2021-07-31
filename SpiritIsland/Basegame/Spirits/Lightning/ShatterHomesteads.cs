using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class ShatterHomesteads {
		public const string Name = "Shatter Homesteads";

		[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
		[FromSacredSite(2)]
		static public async Task Act(ActionEngine engine, Space target){
			var (_,gameState) = engine;
			// Destroy 1 town
			var grp = gameState.InvadersOn(target);
			if(grp.HasTown){
				var town = grp[Invader.Town] > 0 ? Invader.Town : Invader.Town1;
				grp.Destroy(town);
				gameState.UpdateFromGroup(grp);
			}
			// 1 fear
			gameState.AddFear(1);
		}

	}

}
