namespace SpiritIsland;

public class DahanGroupBindingNoEvents {

	public bool Frozen { get; set; }

	readonly protected TokenCountDictionary _tokens;
	readonly protected HealthTokenClass _tokenClass;

	public DahanGroupBindingNoEvents( TokenCountDictionary tokens ) {
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

	public DahanGroupBinding Bind( Guid actionId ) => new DahanGroupBinding(this,actionId); // !!! what about the destroy readon?
}

public class DahanGroupBinding : DahanGroupBindingNoEvents {

	readonly RemoveReason _destroyReason;

	readonly Guid actionId;

	public DahanGroupBinding( DahanGroupBindingNoEvents src, Guid actionId, RemoveReason destroyReason = RemoveReason.Destroyed ):base(src) {
		_destroyReason = destroyReason;
		this.actionId = actionId;
	}

	public DahanGroupBinding( TokenCountDictionary tokens, Guid actionId, RemoveReason destoryReason = RemoveReason.Destroyed ):base(tokens) {
		_destroyReason = destoryReason;
		this.actionId = actionId;
	}

	public async Task AdjustHealthOf( HealthToken token, int delta, int count ) {
		count = Math.Min( _tokens[token], count );
		if(count == 0) return;

		var newToken = token.AddHealth( delta );
		if(newToken.IsDestroyed)
			await this.Destroy( count, token );
		else {
			_tokens.Adjust( token, -count );
			_tokens.Adjust( newToken, count );
		}
	}

	public async Task AdjustHealthOfAll( int delta ) {
		if(delta == 0) return;
		var orderedKeys = delta < 0
			? Keys.OrderBy( x => x.FullHealth ).ToArray()
			: Keys.OrderByDescending( x => x.FullHealth ).ToArray();
		foreach(var t in orderedKeys)
			await AdjustHealthOf( t, delta, _tokens[t] );
	}

	/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
	public Task Add( int count, AddReason reason = AddReason.Added ) {
		return _tokens.AddDefault( TokenType.Dahan, count, actionId, reason );
	}

	/// <summary> Returns the Token removed </summary>
	public async Task<Token> Remove1( RemoveReason reason ) {
		if(Frozen) return null;

		var toRemove = Keys.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		if( toRemove != null)
			await _tokens.Remove( toRemove, 1, actionId, reason );
		return toRemove;
	}

	public async Task<bool> Remove1( Token desiredToken, RemoveReason reason ) {
		if( !Frozen && 0<_tokens[desiredToken] ){ 
			await _tokens.Remove(desiredToken,1, actionId, reason );
			return true;
		}

		return false; // unable to remove desired token
	}

	public void Drown() {

		// !!! need to change this to async so we publish the .Destroy event and UI will update.

		if(Frozen) return;
		foreach( var token in Keys.ToArray() )
			_tokens.Init(token,0);
	}

	#region Damage

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)
		if(Frozen) return;

		var before = Keys.OrderByDescending(x=>x.Damage).ToArray(); // most damaged to least damaged so they don't cascade
		foreach(var token in before) {
			int origCount = _tokens[token];
			var newToken = token.AddDamage( 1 );
			if( newToken.IsDestroyed) {
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

	public async Task Destroy( int count, HealthToken original ) {
		if(Frozen) return;

		await _tokens.Remove( original, count, actionId, _destroyReason );
	}

	#endregion
}