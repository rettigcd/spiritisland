namespace SpiritIsland;

public interface IReactToLandFear {
	void HandleFearAdded( Space space, int fearAdded, FearType fearType );
}
