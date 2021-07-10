using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
	public class ShatterHomesteads : BaseAction {
		public const string Name = "Shatter Homesteads";

		// range 2 from sacred site
		public ShatterHomesteads(Spirit spirit,GameState gameState):base(gameState){_=Act(spirit);}

		async Task Act(Spirit spirit){
			var space = await engine.SelectSpace("Select target land",spirit.SacredSites.Range(2));
			// Destroy 1 town
			var grp = gameState.InvadersOn(space);
			if(grp.HasTown){
				var town = grp[Invader.Town] > 0 ? Invader.Town : Invader.Town1;
				grp[town]--;
				gameState.UpdateFromGroup(grp);
			}
			// 1 fear
			gameState.AddFear(1);
		}

	}

}
