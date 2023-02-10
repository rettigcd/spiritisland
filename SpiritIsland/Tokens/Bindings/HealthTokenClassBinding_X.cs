namespace SpiritIsland;

public class HealthTokenClassBinding {

	readonly protected SpaceState _tokens;
	readonly protected HumanTokenClass _tokenClass;

	public HealthTokenClassBinding( SpaceState tokens, HumanTokenClass tokenClass ) {
		_tokens = tokens;
		_tokenClass = tokenClass;
	}

	public HealthTokenClassBinding( HealthTokenClassBinding src ) {
		_tokens = src._tokens;
		_tokenClass = Human.Dahan;
	}

	#region All TokenCategory.Dahan, including Frozen/Stasis
	public bool Any => _tokens.Has( TokenCategory.Dahan );	// This method canNOT be repurposed for Invaders because multiple share the .Invader category
	public int CountAll => _tokens.Sum( TokenCategory.Dahan );
	#endregion

	/// <summary> All of the Normal Tokens (not frozen, dream) </summary>
	public HumanToken[] NormalKeys => _tokens.OfHumanClass( _tokenClass );

	public void Init( int count ) => _tokens.InitDefault( Human.Dahan, count );

	public void Adjust( ISpaceEntity token, int delta ) => _tokens.Adjust( token, delta );

	public void Init( ISpaceEntity token, int count ) => _tokens.Init( token, count );

	/// <summary> Adds a Token from the bag, or out of thin air. </summary>
	public Task Add( int count, AddReason reason = AddReason.Added ) {
		return _tokens.AddDefault( Human.Dahan, count, reason );
	}

	// Called from .Move() and .Dissolve the Bonds
	public async Task<ISpaceEntity> Remove1( IToken toRemove, RemoveReason reason ) {
		if(_tokens[toRemove] == 0)
			return null; // unable to remove desired token

		await _tokens.Remove( toRemove, 1, reason );
		return toRemove;
	}

	#region Damage

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)
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
				await DestroyToken( token, origCount );
		}

	}

	/// <summary>Applies Damage Inefficiently</summary>
	public async Task ApplyDamage_Inefficiently( int remainingDamageToDahan ) {

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan,
		// you may choose how that Damage is allocated, just like when you Damage Invaders.

		HumanToken mostHealthy = null;
		while(0 < remainingDamageToDahan
			&& (mostHealthy = NormalKeys.OrderByDescending( x => x.RemainingHealth ).FirstOrDefault()) != null // least health first.
		) {
			// determine # to apply 1 damage 2
			int countToApply1DamageTo = Math.Min( remainingDamageToDahan, _tokens[mostHealthy] );
			remainingDamageToDahan -= countToApply1DamageTo;

			HumanToken damagedToken = mostHealthy.AddDamage( 1 );
			if(damagedToken.IsDestroyed) {
				await DestroyToken( mostHealthy, countToApply1DamageTo );
			} else {
				_tokens.Adjust( mostHealthy, -countToApply1DamageTo );
				_tokens.Adjust( damagedToken, countToApply1DamageTo );
			}
		}

	}

	/// <summary>Applies Damage Efficiently</summary>
	/// <returns>Remaining/unused damage</returns>
	public async Task<int> ApplyDamage_Efficiently( int remainingDamageToDahan, HumanToken token ) {
		// Destroy what can be destroyed
		if(token.RemainingHealth <= remainingDamageToDahan) {
			int countDestroyed = remainingDamageToDahan / token.RemainingHealth;
			await DestroyToken( token, countDestroyed );
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

	// !!! This destroy is using Least-Efficient, for Invaders, we want this to be MOST efficient
	public async Task Destroy( int countToDestroy ) {

		var before = NormalKeys
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyed = Math.Min( countToDestroy, _tokens[token] );
			await DestroyToken( token, destroyed );
			countToDestroy -= destroyed;
		}
	}

	public virtual async Task<int> DestroyToken( HumanToken token, int count ) {
		return await token.Destroy( _tokens, count );
	}

	public virtual async Task DestroyAll() {
		foreach(HumanToken token in NormalKeys.ToArray())
			await token.DestroyAll( _tokens );
	}

	#endregion

}
