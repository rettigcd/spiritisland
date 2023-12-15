namespace SpiritIsland;

// Preferred (faster than using await)
public interface IHandleTokenRemoved {
	void HandleTokenRemoved( SpaceState from, ITokenRemovedArgs args );
}

// Allows awaiting when needed
public interface IHandleTokenRemovedAsync {
	Task HandleTokenRemovedAsync( SpaceState from, ITokenRemovedArgs args );
}
