namespace SpiritIsland;

public class TokenAddingHandler : SelfCleaningToken, IHandleAddingToken {

	readonly Action<AddingTokenArgs> _action;

	public TokenAddingHandler( Action<AddingTokenArgs> action ) : base() {
		_action = action;
	}

	public void ModifyAdding( AddingTokenArgs args ) => _action( args );
}
