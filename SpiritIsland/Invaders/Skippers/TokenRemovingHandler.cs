namespace SpiritIsland;

interface IModifyRemoving {
	void ModifyRemoving( RemovingTokenArgs args );
}

public class TokenRemovingHandler : BaseModToken, IModifyRemoving {

	readonly Action<RemovingTokenArgs> _action;

	public TokenRemovingHandler(Action<RemovingTokenArgs> action):base( "modify removing", UsageCost.Free ) {
		_action = action;
	}

	public void ModifyRemoving( RemovingTokenArgs args ) => _action(args);
}