namespace SpiritIsland;

public interface IHandleTokenAdded {
	void HandleTokenAdded( SpaceState to, ITokenAddedArgs args );
}

public interface IHandleTokenAddedAsync {
	Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args );
}

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( SpaceState tokens, int damageAdded );
}