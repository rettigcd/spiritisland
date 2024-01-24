namespace SpiritIsland;

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( SpaceState tokens, int damageAdded );
}

public interface IModifyLandDamage {
	void ModifyLandDamage( SpaceState spaceState, ref int damage );
}