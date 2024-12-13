namespace SpiritIsland;

public interface IReactToLandFear {
	Task HandleFearAddedAsync(Space space, int fearAdded, FearType fearType);
}
