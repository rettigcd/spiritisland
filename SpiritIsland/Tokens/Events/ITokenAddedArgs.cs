namespace SpiritIsland;

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

public interface ITokenAddedArgs {

	#region Possibly Remove these
	public IToken Added { get; } // need specific so we can act on it (push/damage/destroy)
	public Space To { get; }
	#endregion

	/// <summary> The combined type and space of the token AFTER it was added. </summary>
	public SpaceToken After { get; }

	public int Count { get; }
	public AddReason Reason { get; }
}


