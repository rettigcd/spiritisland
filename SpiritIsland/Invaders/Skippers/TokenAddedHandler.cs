namespace SpiritIsland;

public interface IHandleTokenAdded {
	Task HandleTokenAdded( ITokenAddedArgs args );
}

public class TokenAddedHandler : SelfCleaningToken, IHandleTokenAdded {
	readonly Func<ITokenAddedArgs, Task> _func;
	readonly Action<ITokenAddedArgs> _action;
	public TokenAddedHandler(Action<ITokenAddedArgs> handler, bool keepForever = false) : base(keepForever) {
		_action = handler;
	}
	public TokenAddedHandler(Func<ITokenAddedArgs, Task> handler, bool keepForever = false) : base(keepForever) {
		_func = handler;
	}

	async Task IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		if( _func != null)
			await _func( args );
		else
			_action( args );
	}
}
