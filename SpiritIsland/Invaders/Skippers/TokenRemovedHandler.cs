namespace SpiritIsland;

public interface IHandleTokenRemoved {
	Task HandleTokenRemoved( ITokenRemovedArgs args );
}

public class TokenRemovedHandler : BaseModToken, IHandleTokenRemoved {
	readonly Func<ITokenRemovedArgs, Task> _func;
	readonly Action<ITokenRemovedArgs> _action;
	public TokenRemovedHandler( string label, Action<ITokenRemovedArgs> handler, bool keepForever = false ) : base( label, UsageCost.Free, keepForever ) {
		_action = handler;
	}
	public TokenRemovedHandler( string label, Func<ITokenRemovedArgs, Task> handler, bool keepForever = false ) : base( label, UsageCost.Free, keepForever ) {
		_func = handler;
	}

	async Task IHandleTokenRemoved.HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(_func != null)
			await _func( args );
		else
			_action( args );
	}
}