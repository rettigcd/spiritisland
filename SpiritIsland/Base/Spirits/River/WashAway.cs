using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class WashAway {

		public const string Name = "Wash Away";

		[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
		static public async Task ActionAsync(ActionEngine engine){
			var (self,gameState) = engine;
			bool HasTownOrExplorer(Space space){
				var sum = gameState.InvadersOn(space);
				return sum.HasExplorer || sum.HasTown;
			}

			var target = await engine.SelectSpace("Select target space."
				,self.Presence.Range(1).Where(HasTownOrExplorer)
			);

			var group = gameState.InvadersOn(target);
			int numToPush = 3;
			var availableInvaders = group.Filter("T@2","T@1","E@1");
			while(numToPush > 0 && availableInvaders.Length>0){
				var invader = await engine.SelectInvader("Select invader to push."
					,availableInvaders
					,true
				);
				if(invader == null) break;
				var destination = await engine.SelectSpace("Select destination for "+invader.Summary
					,target.Neighbors.Where(x=>x.IsLand),false
				);

				--numToPush;
				group[invader]--;
				new MoveInvader(invader, group.Space, destination).Apply(gameState);
				availableInvaders = group.Filter("T@2","T@1","E@1");
			}
		}

	}

}
