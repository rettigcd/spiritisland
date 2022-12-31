namespace SpiritIsland;

public interface IHandleTokenAdded {
	Task HandleTokenAdded( ITokenAddedArgs args );
}

public class TokenAddedHandler : BaseModToken, IHandleTokenAdded {
	readonly Func<ITokenAddedArgs, Task> _func;
	readonly Action<ITokenAddedArgs> _action;
	public TokenAddedHandler( string label, Action<ITokenAddedArgs> handler, bool keepForever = false ):base(label, UsageCost.Free, keepForever ) {
		_action = handler;
	}
	public TokenAddedHandler( string label, Func<ITokenAddedArgs, Task> handler, bool keepForever = false ) : base( label, UsageCost.Free, keepForever ) {
		_func = handler;
	}

	async Task IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		if( _func != null)
			await _func( args );
		else
			_action( args );
	}
}
