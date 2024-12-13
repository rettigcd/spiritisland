namespace SpiritIsland;

// Allows awaiting when needed
public interface IHandleTokenRemoved {
	Task HandleTokenRemovedAsync( ITokenRemovedArgs args );
}
