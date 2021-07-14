using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
	public class ShatterHomesteads : BaseAction {
		public const string Name = "Shatter Homesteads";

		// range 2 from sacred site
		public ShatterHomesteads(Spirit spirit,GameState gameState):base(spirit,gameState){_=Act();}

		async Task Act(){
			var space = await engine.Api.TargetSpace_SacredSite(2);
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
