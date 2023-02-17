namespace SpiritIsland;

public class TokenRemovedHandlerAsync : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemovedAsync {

	readonly Func<ITokenRemovedArgs, Task> _func;

	public TokenRemovedHandlerAsync( Func<ITokenRemovedArgs, Task> handler ) : base() {
		_func = handler;
	}

	Task IHandleTokenRemovedAsync.HandleTokenRemovedAsync( ITokenRemovedArgs args ) => _func( args );

}

public class TokenRemovedHandlerAsync_Persistent : BaseModEntity, IHandleTokenRemovedAsync {

	readonly Func<ITokenRemovedArgs, Task> _func;

	public TokenRemovedHandlerAsync_Persistent( Func<ITokenRemovedArgs, Task> handler ) {
		_func = handler;
	}

	Task IHandleTokenRemovedAsync.HandleTokenRemovedAsync( ITokenRemovedArgs args ) => _func( args );

}


public class TokenRemovedHandler : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved {

	readonly Action<ITokenRemovedArgs> _action;

	public TokenRemovedHandler( Action<ITokenRemovedArgs> handler ) : base() {
		_action = handler;
	}

	void IHandleTokenRemoved.HandleTokenRemoved( ITokenRemovedArgs args ) {
		_action( args );
	}
}