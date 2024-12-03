namespace SpiritIsland;

// Allows awaiting when needed
public interface IHandleTokenRemoved {
	Task HandleTokenRemovedAsync( Space from, ITokenRemovedArgs args );
}
