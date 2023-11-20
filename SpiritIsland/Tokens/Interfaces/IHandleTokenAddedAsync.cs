namespace SpiritIsland;

public interface IHandleTokenAdded {
	void HandleTokenAdded( ITokenAddedArgs args );
}

public interface IHandleTokenAddedAsync {
	Task HandleTokenAddedAsync( ITokenAddedArgs args );
}

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( SpaceState tokens, int damageAdded );
}