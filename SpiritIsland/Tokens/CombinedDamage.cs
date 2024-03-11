namespace SpiritIsland;

/// <summary>
/// Combines Original & Bonus Damage for the Action based on # of Badlands and Spirit-bonus.
/// </summary>
public class CombinedDamage {

	/// <param name="originalDamage">0, for Apply-X-Damage-To-N </param>
	public CombinedDamage( int originalDamage, params DamagePool[] pools ) {

		_pools = pools;
		_originalDamage = originalDamage;

		Available = originalDamage + _pools.Sum(p=>p.Remaining);
	}


	public int Available { get; }

	public void TrackDamageDone( int damageApplied ) {

		// Remove bonus damage from damage pools
		int poolDamageToAccountFor = damageApplied - _originalDamage;

		foreach(var pool in _pools)
			poolDamageToAccountFor -= pool.ReducePoolDamage( poolDamageToAccountFor );

		if(0 < poolDamageToAccountFor)
			throw new Exception( "somehow we did more damage than we have available" );
	}

	#region private
	readonly int _originalDamage;
	readonly DamagePool[] _pools;
	#endregion

}

static public class CombinedDamage_Extenstions {

	/// <summary>
	/// Bonus damage to Invaders when damage was caused by Do-X-Damage-To-N invaders. (so no 'used' damage to supply
	/// </summary>
	static public CombinedDamage CombinedDamageFor_Invaders( this Space space ) {
		return new CombinedDamage( 0,
			DamagePool.BadlandDamage( space, "Invaders" ), // use badland damage 1st since it is localized to a space
			DamagePool.SpiritsBonusDamage()                 // not localized to a space
		);
	}

	/// <summary>
	/// Bonus damage to Invaders when original damage is required.
	/// </summary>
	/// <param name="originalDamage">This damage will not be removed from bonus damage pools.</param>
	/// <remarks>If bonus damage doesn't apply, calleris responsible for NOT calling this.</remarks>
	static public CombinedDamage CombinedDamageFor_Invaders( this Space space, int originalDamage ) {
		return originalDamage == 0 
			? new CombinedDamage(0)
			: new CombinedDamage( originalDamage,
				DamagePool.BadlandDamage( space, "Invaders" ), // use badland damage 1st since it is localized to a space
				DamagePool.SpiritsBonusDamage()                 // not localized to a space
			);
	}

	static public CombinedDamage CombinedDamageFor_Dahan( this Space space, int originalDamage = 0 )
		=> new CombinedDamage( originalDamage,
			DamagePool.BadlandDamage( space, "Dahan" )
		);

}