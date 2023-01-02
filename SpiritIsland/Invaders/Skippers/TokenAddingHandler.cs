namespace SpiritIsland;

interface IHandleAddingToken {
	void ModifyAdding( AddingTokenArgs args );
}

public class TokenAddingHandler : BaseModToken, IHandleAddingToken {

	readonly Action<AddingTokenArgs> _action;

	public TokenAddingHandler( Action<AddingTokenArgs> action ) : base( "modify adding", UsageCost.Free ) {
		_action = action;
	}

	public void ModifyAdding( AddingTokenArgs args ) => _action( args );
}
