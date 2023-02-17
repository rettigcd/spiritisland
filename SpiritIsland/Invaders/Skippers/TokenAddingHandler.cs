namespace SpiritIsland;

public class TokenAddingHandler : BaseModEntity, IEndWhenTimePasses, IModifyAddingToken {

	readonly Action<AddingTokenArgs> _action;

	public TokenAddingHandler( Action<AddingTokenArgs> action ) : base() {
		_action = action;
	}

	public void ModifyAdding( AddingTokenArgs args ) { 
		_action( args );
	}
}
