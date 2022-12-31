namespace SpiritIsland;

public interface IHandleTokenRemoved {
	Task HandleTokenRemoved( TokenRemovedArgs args );
}

public class TokenRemovedHandler : BaseModToken, IHandleTokenRemoved {
	readonly Func<TokenRemovedArgs, Task> _func;
	readonly Action<TokenRemovedArgs> _action;
	public TokenRemovedHandler( string label, Action<TokenRemovedArgs> handler, bool keepForever = false ) : base( label, UsageCost.Free, keepForever ) {
		_action = handler;
	}
	public TokenRemovedHandler( string label, Func<TokenRemovedArgs, Task> handler, bool keepForever = false ) : base( label, UsageCost.Free, keepForever ) {
		_func = handler;
	}

	async Task IHandleTokenRemoved.HandleTokenRemoved( TokenRemovedArgs args ) {
		if(_func != null)
			await _func( args );
		else
			_action( args );
	}
}