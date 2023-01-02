namespace SpiritIsland;


public class PublishTokenRemovedArgs {

	public PublishTokenRemovedArgs( Token token, RemoveReason reason, UnitOfWork actionId, SpaceState space, int count ) {
		Token = token;
		_reason = reason;
		_action = actionId;
		_space = space;
		Count = count;
	}

	public Token Token { get; }
	public int Count { get; }
	readonly SpaceState _space;
	readonly RemoveReason _reason;
	readonly UnitOfWork _action;

	public TokenRemovedArgs MakeEvent() => new TokenRemovedArgs(Token,_reason,_action,_space,Count);
}


/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(Token token, RemoveReason reason, UnitOfWork action, SpaceState space, int count ) {
		Token = token;
		Reason = reason;
		Action = action ?? throw new ArgumentNullException(nameof(action));
		Space = space;
		Count = count;
	}
	public Token Token { get; }
	public SpaceState Space { get; set; } // !!! why are these settable?
	public int Count { get; set; }			// !!! set???
	public RemoveReason Reason { get; }
	public UnitOfWork Action { get; }
}

