namespace SpiritIsland;

public interface IHandleTokenAdded {
	void HandleTokenAdded( Space to, ITokenAddedArgs args );
}

public interface IHandleTokenAddedAsync {
	Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args );
}
