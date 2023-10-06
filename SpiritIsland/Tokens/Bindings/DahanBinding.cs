namespace SpiritIsland;

public sealed class DahanBinding {

	readonly SpaceState _tokens;

	public DahanBinding( SpaceState tokens ) {
		_tokens = tokens;
	}

	#region All TokenCategory.Dahan, including Frozen/Stasis

	/// <summary> Includes Frozen and dream dahan. </summary>
	public bool Any => _tokens.Has( TokenCategory.Dahan );
	public int CountAll => _tokens.Sum( TokenCategory.Dahan );

	#endregion

	/// <summary> The non-frozes,non-dreaming Tokens of various health and damage.</summary>
	public HumanToken[] NormalKeys => _tokens.OfHumanClass( Human.Dahan );

	public void Init( int count ) => _tokens.InitDefault( Human.Dahan, count );

	public void Adjust( ISpaceEntity token, int delta ) => _tokens.Adjust( token, delta );

	/// <summary> Adds a Token from the bag, or out of thin air. </summary>
	public Task AddDefault( int count, AddReason reason = AddReason.Added )
		=> _tokens.AddDefault( Human.Dahan, count, reason );

	// Called from .Move() and .Dissolve the Bonds
	public async Task<TokenRemovedArgs> Remove1( IToken toRemove, RemoveReason reason ) {
		return await _tokens.Remove( toRemove, 1, reason );
	}

	#region Damage

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)
		if(_tokens.ModsOfType<IStopDahanDamage>().Any()) return;

		var before = NormalKeys
			.OrderByDescending( x => x.Damage ) // most damaged to least damaged so they don't cascade
			.ToArray();

		foreach(var token in before) {
			int origCount = _tokens[token];
			var newToken = token.AddDamage( 1 );
			if(!newToken.IsDestroyed) {
				// Apply 1 damage to all
				_tokens.Init( token, 0 );
				_tokens.Adjust( newToken, origCount );
			} else
				// or destory
				await Destroy( origCount, token );
		}

	}

	/// <summary>Applies Damage Inefficiently</summary>
	public async Task<int> ApplyDamage_Inefficiently( int remainingDamageToDahan ) {
		if(_tokens.ModsOfType<IStopDahanDamage>().Any()) return 0;

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan,
		// you may choose how that Damage is allocated, just like when you Damage Invaders.

		int total = 0;

		HumanToken mostHealthy = null;
		while(0 < remainingDamageToDahan
			&& (mostHealthy = NormalKeys.OrderByDescending( x => x.RemainingHealth ).FirstOrDefault()) != null // least health first.
		) {
			// determine # to apply 1 damage 2
			int countToApply1DamageTo = Math.Min( remainingDamageToDahan, _tokens[mostHealthy] );
			remainingDamageToDahan -= countToApply1DamageTo;
			total += countToApply1DamageTo;

			HumanToken damagedToken = mostHealthy.AddDamage( 1 );
			if(damagedToken.IsDestroyed) {
				await Destroy( countToApply1DamageTo, mostHealthy );
			} else {
				_tokens.Adjust( mostHealthy, -countToApply1DamageTo );
				_tokens.Adjust( damagedToken, countToApply1DamageTo );
			}
		}

		return total;
	}

	/// <summary>Applies Damage Efficiently</summary>
	/// <returns>Remaining/unused damage</returns>
	public async Task<int> ApplyDamage_Efficiently( int remainingDamageToDahan, HumanToken token ) {
		if(_tokens.ModsOfType<IStopDahanDamage>().Any()) return remainingDamageToDahan;

		// Destroy what can be destroyed
		if(token.RemainingHealth <= remainingDamageToDahan) {
			int countDestroyed = remainingDamageToDahan / token.RemainingHealth;
			await Destroy( countDestroyed, token );
			remainingDamageToDahan -= countDestroyed * token.RemainingHealth;
		}
		// if there is still partial damage we can apply
		if(0 < remainingDamageToDahan && 0 < _tokens[token]) {
			_tokens.Adjust( token, -1 );
			_tokens.Adjust( token.AddDamage( remainingDamageToDahan ), 1 );
			remainingDamageToDahan = 0; // damage should be used up
		}

		return remainingDamageToDahan;
	}

	#endregion

	#region Destroy

	// This destroy is using Least-Efficient (dahan-only)
	// for Invaders, we would want this to be MOST efficient
	public async Task Destroy( int countToDestroy ) {

		var before = NormalKeys
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyCountToApplyToThisToken = Math.Min( countToDestroy, _tokens[token] );
			countToDestroy -= destroyCountToApplyToThisToken; // doesn't matter how many were actually destroyed.
			await Destroy(destroyCountToApplyToThisToken, token);
		}
	}

	/// <returns># of tokens destroyed</returns>
	public async Task<int> Destroy( int count, HumanToken token ) {
		// ! Running this through the Dahan.Destroy is unnessary since it is never overriden.
		// However, this matches the Invader pattern which IS necessary due to Habsburg Durable tokens.
		return await token.Destroy( _tokens, count );
	}

	public async Task DestroyAll() {
		foreach(HumanToken token in NormalKeys.ToArray())
			await token.DestroyAll( _tokens );
	}

	#endregion

}
