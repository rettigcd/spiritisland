namespace SpiritIsland.PowerCards {

	// Wraps action, provides link to power card that generated the action.
	public class CardAction : IAction {

		public CardAction(PowerCard powerCard, IAction action){
			Card = powerCard;
			this.InnerAction = action;
		}

		public PowerCard Card { get; }

		public readonly IAction InnerAction;


		public bool IsResolved => InnerAction.IsResolved;

		public void Apply() => InnerAction.Apply();
	}

}



