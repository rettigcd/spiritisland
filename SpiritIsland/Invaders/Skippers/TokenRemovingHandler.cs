namespace SpiritIsland;

public interface IHandleRemovingToken {
	Task ModifyRemoving( RemovingTokenArgs args );
}

public class TokenRemovingHandler : BaseModToken, IHandleRemovingToken {

	readonly Action<RemovingTokenArgs> _action;
	readonly Func<RemovingTokenArgs,Task> _func;

	public TokenRemovingHandler(Action<RemovingTokenArgs> action):base() {
		_action = action;
	}
	public TokenRemovingHandler( Func<RemovingTokenArgs,Task> func ) : base() {
		_func = func;
	}

	public async Task ModifyRemoving( RemovingTokenArgs args ) {
		if( _func != null )
			await _func(args);
		else
			_action(args);
	}
}