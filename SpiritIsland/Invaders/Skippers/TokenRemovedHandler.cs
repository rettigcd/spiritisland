namespace SpiritIsland;

public interface IHandleTokenRemoved {
	Task HandleTokenRemoved( ITokenRemovedArgs args );
}

public class TokenRemovedHandler : SelfCleaningToken, IHandleTokenRemoved {
	readonly Func<ITokenRemovedArgs, Task> _func;
	readonly Action<ITokenRemovedArgs> _action;
	public TokenRemovedHandler( Action<ITokenRemovedArgs> handler, bool keepForever = false ) : base( keepForever ) {
		_action = handler;
	}
	public TokenRemovedHandler( Func<ITokenRemovedArgs, Task> handler, bool keepForever = false ) : base( keepForever ) {
		_func = handler;
	}

	async Task IHandleTokenRemoved.HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(_func != null)
			await _func( args );
		else
			_action( args );
	}
}