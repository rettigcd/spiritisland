namespace SpiritIsland;

interface IModifyRemoving {
	void ModifyRemoving( RemovingTokenArgs args );
}

public class RemovingHandlerToken : ActionModBaseToken, IModifyRemoving {

	readonly Action<RemovingTokenArgs> _action;

	public RemovingHandlerToken(Action<RemovingTokenArgs> action):base("modify removing" ) {
		_action = action;
	}

	public void ModifyRemoving( RemovingTokenArgs args ) => _action(args);
}
