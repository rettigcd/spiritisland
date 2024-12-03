namespace SpiritIsland;

public interface IHandleTokenAdded {
	Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args );
}
