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
	SpaceState _space;
	RemoveReason _reason;
	UnitOfWork _action;

	public TokenRemovedArgs MakeEvent( GameState gameState ) => new TokenRemovedArgs(Token,_reason,_action,_space,Count,gameState);
}


/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(Token token, RemoveReason reason, UnitOfWork actionId, SpaceState space, int count, GameState gs ) {
		Token = token;
		Reason = reason;
		Action = actionId;
		Space = space;
		Count = count;
		GameState = gs;
	}
	public Token Token { get; }
	public SpaceState Space { get; set; } // !!! why are these settable?
	public int Count { get; set; }			// !!! set???
	public RemoveReason Reason { get; }
	public UnitOfWork Action { get; }
	public GameState GameState { get; }
}

