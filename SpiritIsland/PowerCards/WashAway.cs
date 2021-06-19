namespace SpiritIsland.PowerCards {

	[PowerCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : BaseAction, IAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gameState):base(gameState) {
			engine.decisions.Push( new TargetSpaceRangeFromPresence(spirit,1
				,HasExplorersOrInvaders
				,Push3FromSpace
			) );
			AutoSelectSingleOptions();
		}

		bool HasExplorersOrInvaders(Space space){
			var sum = gameState.GetInvaderGroup(space);
			return sum.HasExplorer || sum.HasTown;
		}

		void Push3FromSpace(Space space,ActionEngine engine){
			var targetGroup = gameState.GetInvaderGroup(space);
			engine.decisions.Push(new SelectTownAndExplorersToPush(targetGroup,space,3));
		}

	}

}
