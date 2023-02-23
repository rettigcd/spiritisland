namespace SpiritIsland;

public class DamagePool {

	public static DamagePool BadlandDamage( SpaceState ss, string groupName ) {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BadlandDamage_" + ss.Space.Label +"_" + groupName;
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( ss.Badlands.Count );
		actionScope[key] = pool;
		return pool;
	}

	public static DamagePool BonusDamage() {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BonusDamage";
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( actionScope?.Owner?.BonusDamage ?? 0 );
		actionScope[key] = pool;
		return pool;
	}

	public DamagePool( int init ) { remaining = init; }

	public int ReducePoolDamage( int poolDamageToAccountFor ) {
		int damageFromBadlandPool = Math.Min( remaining, poolDamageToAccountFor );
		remaining -= damageFromBadlandPool;
		return damageFromBadlandPool;
	}

	int remaining;
	public int Remaining => remaining;
}
