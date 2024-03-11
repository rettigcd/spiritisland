namespace SpiritIsland;

// Preferred (faster than using await)
public interface IHandleTokenRemoved {
	void HandleTokenRemoved( Space from, ITokenRemovedArgs args );
}

// Allows awaiting when needed
public interface IHandleTokenRemovedAsync {
	Task HandleTokenRemovedAsync( Space from, ITokenRemovedArgs args );
}
