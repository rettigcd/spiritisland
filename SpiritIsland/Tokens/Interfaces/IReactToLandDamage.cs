namespace SpiritIsland;

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( Space space, int damageAdded );
}

public interface IModifyLandDamage {
	void ModifyLandDamage( Space space, ref int damage );
}