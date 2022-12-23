namespace SpiritIsland;

public class DahanGroupBindingNoEvents {

	readonly protected SpaceState _tokens;
	readonly protected HealthTokenClass _tokenClass;

	public DahanGroupBindingNoEvents( SpaceState tokens ) {
		_tokens = tokens;
		_tokenClass = TokenType.Dahan;
	}

	public DahanGroupBindingNoEvents( DahanGroupBindingNoEvents src ) {
		_tokens = src._tokens;
		_tokenClass = TokenType.Dahan;
	}

	#region All TokenCategory.Dahan, including Frozen/Stasis
	public bool Any => _tokens.Has( TokenCategory.Dahan );
	public int CountAll => _tokens.Sum( TokenCategory.Dahan );
	#endregion

	/// <summary> All of the Normal Tokens (not frozen, dream, stasis) </summary>
	public HealthToken[] NormalKeys => _tokens.OfHealthClass( _tokenClass );

	public void Init( int count ) => _tokens.InitDefault( TokenType.Dahan, count );

	public void Adjust( Token token, int delta ) => _tokens.Adjust( token, delta );

	public void Init( Token token, int count ) => _tokens.Init( token, count );

	public DahanGroupBinding Bind( UnitOfWork actionId ) => new DahanGroupBinding(this,actionId);
}

public class DahanGroupBinding : DahanGroupBindingNoEvents {

	readonly RemoveReason _destroyReason;

	readonly UnitOfWork actionId;

	#region constructors

	public DahanGroupBinding( DahanGroupBindingNoEvents src, UnitOfWork actionId, RemoveReason destroyReason = RemoveReason.Destroyed ):base(src) {
		_destroyReason = destroyReason;
		this.actionId = actionId;
	}

	public DahanGroupBinding( SpaceState tokens, UnitOfWork actionId, RemoveReason destoryReason = RemoveReason.Destroyed ):base(tokens) {
		_destroyReason = destoryReason;
		this.actionId = actionId;
	}

	#endregion

	/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
	public Task Add( int count, AddReason reason = AddReason.Added ) {
		return _tokens.AddDefault( TokenType.Dahan, count, actionId, reason );
	}

	// Called from .Move() and .Dissolve the Bonds
	public async Task<Token> Remove1( RemoveReason reason, Token toRemove=null ) {

		// Reason is only MovedFrom and Replaced.  No destroy here

		// validate token to be removed.
		if( toRemove == null )
			toRemove = NormalKeys.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		else if( _tokens[toRemove] == 0 )
			toRemove = null; // unable to remove desired token

		if(toRemove != null)
			await _tokens.Remove( toRemove, 1, actionId, reason );
		return toRemove;
	}

	// Called from Ocean-Drown special rule.
	public async Task Drown() {
		foreach( HealthToken token in NormalKeys)
			await Destroy( _tokens[token], token );
	}

	#region Damage

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)
		var before = NormalKeys
			.OrderByDescending(x=>x.Damage) // most damaged to least damaged so they don't cascade
			.ToArray(); 
		foreach(var token in before) {
			int origCount = _tokens[token];
			var newToken = token.AddDamage( 1 );
			if( !newToken.IsDestroyed ) {
				// Apply 1 damage to all
				_tokens.Init( token, 0 );
				_tokens.Adjust( newToken, origCount );
			} else
				// or destory
				await Destroy( origCount, token );
		}

	}

	/// <summary>Applies Damage Inefficiently</summary>
	public async Task ApplyDamage_Inefficiently( int remainingDamageToDahan ) {

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan,
		// you may choose how that Damage is allocated, just like when you Damage Invaders.

		HealthToken mostHealthy = null;
		while( 0<remainingDamageToDahan 
			&& (mostHealthy=NormalKeys.OrderByDescending( x => x.RemainingHealth ).FirstOrDefault()) != null // least health first.
		) {
			// determine # to apply 1 damage 2
			int countToApply1DamageTo = Math.Min(remainingDamageToDahan, _tokens[mostHealthy]);
			remainingDamageToDahan -= countToApply1DamageTo;

			HealthToken damagedToken = mostHealthy.AddDamage( 1 );
			if(damagedToken.IsDestroyed) {
				await Destroy( countToApply1DamageTo, mostHealthy );
			} else {
				_tokens.Adjust( mostHealthy, -countToApply1DamageTo );
				_tokens.Adjust( damagedToken, countToApply1DamageTo );
			}
		}

	}

	/// <summary>Applies Damage Efficiently</summary>
	/// <returns>Remaining/unused damage</returns>
	public async Task<int> ApplyDamage_Efficiently( int remainingDamageToDahan, HealthToken token ) {
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

	public async Task Destroy( int countToDestroy ) {

		var before = NormalKeys.OrderBy( x => x.RemainingHealth ).ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyed = Math.Min(countToDestroy, _tokens[token]);
			await Destroy( destroyed, token );
			countToDestroy -= destroyed;
		}
	}

	public virtual async Task<PublishTokenRemovedArgs> Destroy( int count, HealthToken original ) {
		return await _tokens.Remove( original, count, actionId, _destroyReason );
	}

	#endregion
}