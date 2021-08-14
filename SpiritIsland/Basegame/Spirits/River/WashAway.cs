using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class WashAway {

		public const string Name = "Wash Away";

		[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
		[FromPresence(1,Target.TownOrExplorer)]
		static public async Task ActionAsync(ActionEngine engine,Space target){
			var (_,gameState) = engine;
			
			int numToPush = 3;
			Invader[] CalcInvaders() => gameState.InvadersOn( target )
				.FilterBy(Invader.Town,Invader.Explorer);
			var availableInvaders = CalcInvaders();
			while(numToPush > 0 && availableInvaders.Length>0){
				var invader = await engine.SelectInvader("Select invader to push."
					,availableInvaders
					,true
				);
				if(invader == null) break;
				await engine.PushInvader(target,invader);

				--numToPush;
				availableInvaders = CalcInvaders();
			}
		}

	}

}
