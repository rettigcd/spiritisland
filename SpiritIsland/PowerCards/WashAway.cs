﻿namespace SpiritIsland.PowerCards {

	[PowerCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : BaseAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gameState):base(gameState) {
			engine.decisions.Push( new TargetSpaceRangeFromPresence(spirit,1
				,HasExplorersOrInvaders // Filter
				,Push3FromSpace
			) );
			AutoSelectSingleOptions();
		}

		bool HasExplorersOrInvaders(Space space){
			var sum = gameState.GetInvaderGroup(space);
			return sum.HasExplorer || sum.HasTown;
		}

		void Push3FromSpace(Space space,ActionEngine engine){
			engine.decisions.Push(new SelectInvadersToPush(
				gameState.GetInvaderGroup(space),3
				,"Town","Explorer"
			));
		}

	}

}
