namespace SpiritIsland;

public interface IReactToLandDamage {
	Task HandleDamageAddedAsync( Space space, int damageAdded );
}

// Used by:
//  IntensifyExploytation (incorrectly)
//  Habsburg Monarchy NeighborTownsCauseBonusLandDamage (incorrectly)
public interface IModifyLandDamage {
	void ModifyLandDamage( Space space, ref int damage );
}

public interface IModifyBlightThreshold {
	void ModifyLandsResilience( Space space, ref int blightThreshold );
}
