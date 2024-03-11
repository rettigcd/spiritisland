namespace SpiritIsland;

/// <summary>
/// Holds 1 set of bonus damage available for the action.
/// </summary>
public class DamagePool( int _remaining ) {

	#region static factories
	static public DamagePool BadlandDamage( Space ss, string groupName ) {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BadlandDamage_" + ss.SpaceSpec.Label +"_" + groupName;
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( ss.Badlands.Count );
		actionScope[key] = pool;
		return pool;
	}

	static public DamagePool SpiritsBonusDamage() {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BonusDamage";
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( actionScope?.Owner?.BonusDamage ?? 0 );
		actionScope[key] = pool;
		return pool;
	}

	#endregion

	public int ReducePoolDamage( int poolDamageToAccountFor ) {
		int damageFromBadlandPool = Math.Min( _remaining, poolDamageToAccountFor );
		_remaining -= damageFromBadlandPool;
		return damageFromBadlandPool;
	}

	public int Remaining => _remaining;

	#region private
	#endregion

}
