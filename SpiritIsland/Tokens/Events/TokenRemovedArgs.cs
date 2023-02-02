namespace SpiritIsland;


public class PublishTokenRemovedArgs {

	public PublishTokenRemovedArgs( IVisibleToken token, RemoveReason reason, UnitOfWork actionScope, SpaceState space, int count ) {
		Token = token;
		_reason = reason;
		_actionScope = actionScope;
		_space = space;
		Count = count;
	}

	public IVisibleToken Token { get; }
	public int Count { get; }
	readonly SpaceState _space;
	readonly RemoveReason _reason;
	readonly UnitOfWork _actionScope;

	public TokenRemovedArgs MakeEvent() => new TokenRemovedArgs(Token,_reason,_actionScope,_space,Count);
}


/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(IVisibleToken token, RemoveReason reason, UnitOfWork action, SpaceState space, int count ) {
		Token = token;
		Reason = reason;
		ActionScope = action ?? throw new ArgumentNullException(nameof(action));
		RemovedFrom = space;
		Count = count;
	}
	public IVisibleToken Token { get; }
	public SpaceState RemovedFrom { get; }
	public int Count { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionScope { get; }
}

