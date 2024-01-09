namespace SpiritIsland;

public interface IReactToLandFear {
	void HandleFearAdded( SpaceState tokens, int fearAdded, FearType fearType );
}
