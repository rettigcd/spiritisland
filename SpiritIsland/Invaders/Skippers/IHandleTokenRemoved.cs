namespace SpiritIsland;

public interface IHandleTokenRemoved {
	Task HandleTokenRemoved( ITokenRemovedArgs args );
}
