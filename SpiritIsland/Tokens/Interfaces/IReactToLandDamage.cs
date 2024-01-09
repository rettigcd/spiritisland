namespace SpiritIsland;

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( SpaceState tokens, int damageAdded );
}
