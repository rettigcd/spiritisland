using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : BaseAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gameState)
			:base(gameState)
		{
			_ = ActionAsync(spirit);
		}

		async Task ActionAsync(Spirit spirit){
			// range 1
			var target = await engine.SelectSpace("Select target space."
				,spirit.Presence.Range(1).Where(HasTownOrExplorer)
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
					,target.SpacesExactly(1).Where(x=>x.IsLand),false
				);

				--numToPush;
				group[invader]--;
				new MoveInvader(invader, group.Space, destination).Apply(gameState);
				availableInvaders = group.Filter("T@2","T@1","E@1");
			}
		}

		//protected void SelectSpace(Space space){
		//	engine.decisions.Push(new SelectInvadersToPush(engine,
		//		gameState.InvadersOn(space),3,true
		//		,"Town","Explorer"
		//	));
		//}

		bool HasTownOrExplorer(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

	}

}
