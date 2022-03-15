namespace SpiritIsland;

// !! Could create a base class Called ITokenChanged that has: Token, count, Space  (maybe combind AddReason and RemoveReason???)
// Then Observice The Ever-Changing world could just subscribe to 1 event

public interface ITokenAddedArgs {
	public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
	public int Count { get; }
	public Space Space { get; }
	public AddReason Reason { get; }
	public GameState GameState { get; }
}

public enum AddReason {
	None, // default / unspecified
	Added, // Generic add
	Explore, // invaders
	Build, // invaders
	AsReplacement, //
	TakenFromCard, // blight only
	MovedTo,
}


public interface ITokenRemovedArgs {
	public Token Token { get; }
	public int Count { get; }
	public Space Space { get;}
	public RemoveReason Reason { get; }
	public GameState GameState { get; }
};

public enum RemoveReason {
	None,
	Removed,           // Generic 'Remove X' command
	UsedUp,            // Wilds, Disease, Strife
	Destroyed,
	DestroyedInBattle, // with invaders - Used by Thunderspeaker's Special Rule
	Replaced,          // with another token
	ReturnedToCard,    // Blight
	MovedFrom 
}

static public class RemoveReasonExtension {
	static public bool IsDestroy(this RemoveReason reason) => reason == RemoveReason.Destroyed || reason == RemoveReason.DestroyedInBattle;
}

public class TokenMovedArgs : ITokenAddedArgs, ITokenRemovedArgs {
	public Token Token { get; set;}
	public int Count { get; set; }
	public GameState GameState { get; set; }

	public Space RemovedFrom { get; set; }
	public Space AddedTo { get; set; }

	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.MovedFrom;

	Space ITokenRemovedArgs.Space => RemovedFrom;

	AddReason ITokenAddedArgs.Reason => AddReason.MovedTo;

	Space ITokenAddedArgs.Space => AddedTo;

}

// 3) ??? Missing events:
// TokenChangedHealth  (or maybe strife)
// TokenRemoved / Replaced

// 4) Instead of passing GameState to all invokers, Invokers just Publish their event and the API finds the correct handlers.

// 5a) Get rid of count? or actuall use it
// 5b) Consolidate multiple events into a single?

// 6) Tracking Invaders?,  Does Move trigger an Add event???

// Idea! -
// 1 Remove from Space event,
// 1 Add to Space event
// 'Move' event implements both Add / Remove interfaces
// if we need a 'Change' event, it can implements both Add / Remove interfaces
