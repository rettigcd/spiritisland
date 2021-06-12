namespace SpiritIsland {

//	// Wraps action, provides link to power card that generated the action.
//	public class CardAction 
//	//	: INamedAction 
//	{

//		public CardAction(PowerCard powerCard, IAction action){
//			Card = powerCard;
//			this.InnerAction = action;
//		}

//		public PowerCard Card { get; }

//		public readonly IAction InnerAction;


//		public bool IsResolved => InnerAction.IsResolved;

////		string INamedAction.Name => Card.Name;

//		public void Apply() => InnerAction.Apply();
//	}

	public class InnateAction : IAction { // :INamedAction
		public InnatePower Innate { get; }

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			throw new System.NotImplementedException();
		}
	}

}



