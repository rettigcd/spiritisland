namespace SpiritIsland;

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( Space space, int damageAdded );
}
