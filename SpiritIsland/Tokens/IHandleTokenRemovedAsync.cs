namespace SpiritIsland;

// Preferred (faster than using await)
public interface IHandleTokenRemoved {
	void HandleTokenRemoved( ITokenRemovedArgs args );
}

// Allows awaiting when needed
public interface IHandleTokenRemovedAsync {
	Task HandleTokenRemovedAsync( ITokenRemovedArgs args );
}
