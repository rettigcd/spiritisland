using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[PowerCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : TargetSpaceAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gameState)
			:base(spirit,gameState,1,From.Presence) {}

		protected override bool FilterSpace(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

		protected override void SelectSpace(Space space){
			engine.decisions.Push(new SelectInvadersToPush(engine,
				gameState.InvadersOn(space),3,false
				,"Town","Explorer"
			));
		}

	}

}
