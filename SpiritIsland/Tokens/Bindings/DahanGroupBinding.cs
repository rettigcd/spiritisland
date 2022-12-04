namespace SpiritIsland;

public class DahanGroupBindingNoEvents {

	public bool Frozen { get; set; }

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

	public IEnumerable<HealthToken> Keys => _tokens.OfType( _tokenClass ).Cast<HealthToken>();

	public bool Any => Count > 0;

	public int Count => _tokens.Sum( _tokenClass );

	public static implicit operator int( DahanGroupBindingNoEvents b ) => b.Count;

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
	public virtual async Task<Token> Remove1( RemoveReason reason, Token toRemove=null ) {
		if( Frozen ) return null; // unable to remove desired token

		// validate token to be removed.
		if( toRemove == null )
			toRemove = Keys.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		else if( _tokens[toRemove] == 0 )
			toRemove = null; // unable to remove desired token

		if(toRemove != null)
			await _tokens.Remove( toRemove, 1, actionId, reason );
		return toRemove;
	}

	// Called from Ocean-Drown special rule.
	public void Drown() {

		// !!! need to change this to async so we publish the .Destroy event and UI will update.

		if(Frozen) return;
		foreach( var token in Keys.ToArray() )
			_tokens.Init(token,0);
	}

	#region Damage

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)
		if(Frozen) return;

		var before = Keys
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

	public async Task ApplyDamage( int damageToDahan ) {
		if(Frozen) return;

		var before = Keys.OrderBy(x=>x.RemainingHealth).ToArray(); // least health first.
		foreach(var token in before) {
			damageToDahan = await ApplyDamageToToken( damageToDahan, token );
			if(damageToDahan == 0)
				break;
		}

	}

	public async Task<int> ApplyDamageToToken( int damageToDahan, HealthToken token ) {
		// Destroy what can be destroyed
		if(token.RemainingHealth <= damageToDahan) {
			int destroyed = damageToDahan / token.RemainingHealth;
			await Destroy( destroyed, token );
			damageToDahan -= destroyed * token.RemainingHealth;
		}
		// if we can apply partial damage
		if(0 < damageToDahan && 0 < _tokens[token]) {
			_tokens.Adjust( token, -1 );
			_tokens.Adjust( token.AddDamage( damageToDahan ), 1 );
			damageToDahan = 0; // damage should be used up
		}

		return damageToDahan;
	}

	#endregion

	#region Destroy

	public async Task Destroy( int countToDestroy ) {
		if(Frozen) return;

		var before = Keys.OrderBy( x => x.RemainingHealth ).ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyed = Math.Min(countToDestroy, _tokens[token]);
			await Destroy( destroyed, token );
			countToDestroy -= destroyed;
		}
	}

	public async Task<PublishTokenRemovedArgs> Destroy( int count, HealthToken original ) {
		return Frozen ? null
			: await _tokens.Remove( original, count, actionId, _destroyReason );
	}

	#endregion
}