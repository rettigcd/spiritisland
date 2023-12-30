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
