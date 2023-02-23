namespace SpiritIsland;

/// <summary>
/// Tracks Bonus Damage for the Action based on # of Badlands and Spirit-bonus.
/// </summary>
public class BonusDamage {
	readonly int _originalDamage;
	readonly DamagePool _badlands;
	readonly DamagePool _bonus;

	/// <summary> May or may not have Original damage.  Track it and acount for it. </summary>
	public BonusDamage( DamagePool badlands, DamagePool bonusDamage, int? originalDamage ) {
		_badlands = badlands;
		_bonus = bonusDamage;

		if(originalDamage.HasValue) {
			// only triggers if there was actual damage done and we need to account for it.
			_originalDamage = originalDamage.Value;

			Available = originalDamage.Value + _bonus.Remaining;
			if(0 < originalDamage)
				Available += _badlands.Remaining;
		} else {
			// Original damage is known to have happened and we don't need to track it.
			_originalDamage = 0;

			Available = _bonus.Remaining + _badlands.Remaining;
		}
	}

	public int Available { get; }

	public void TrackDamageDone( int damageApplied ) {
		// Remove bonus damage from damage pools
		int poolDamageToAccountFor = damageApplied - _originalDamage;
		poolDamageToAccountFor -= _badlands.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= _bonus.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}
}
