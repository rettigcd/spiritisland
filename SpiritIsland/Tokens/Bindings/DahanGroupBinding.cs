namespace SpiritIsland;

public class DahanGroupBinding {

	public bool Frozen { get; set; }

	readonly TokenCountDictionary _tokens;
	readonly HealthTokenClass _tokenGroup;

	public DahanGroupBinding( TokenCountDictionary tokens, RemoveReason destoryReason = RemoveReason.Destroyed ) {
		_tokens = tokens;
		_tokenGroup = TokenType.Dahan;
		_destroyReason = destoryReason;
	}

	readonly RemoveReason _destroyReason;


	public IEnumerable<HealthToken> Keys => _tokens.OfType(_tokenGroup).Cast<HealthToken>();

	public bool Any => Count > 0;

	public int Count => _tokens.Sum(_tokenGroup);

	public static implicit operator int( DahanGroupBinding b ) => b.Count;

	/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
	public Task Add( int count, AddReason reason = AddReason.Added ) {
		return _tokens.AddDefault(TokenType.Dahan,count, reason );
	}

	public void Init(int count ) => _tokens.InitDefault(TokenType.Dahan, count );

	public void Adjust(Token token, int delta ) => _tokens.Adjust(token,delta);

	public void Init(Token token, int count ) => _tokens.Init(token,count);

	/// <summary> Returns the Token removed </summary>
	public async Task<Token> Remove1( RemoveReason reason ) {
		if(Frozen) return null;

		var toRemove = Keys.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		if( toRemove != null)
			await _tokens.Remove( toRemove, 1, reason );
		return toRemove;
	}

	public async Task<bool> Remove1( Token desiredToken, RemoveReason reason ) {
		if( !Frozen && 0<_tokens[desiredToken] ){ 
			await _tokens.Remove(desiredToken,1, reason );
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

	public async Task AdjustHealthOf( HealthToken token, int delta, int count ) {
		count = Math.Min( _tokens[token], count );
		if( count == 0 ) return;

		var newToken = token.AddHealth( delta );
		if( newToken.IsDestroyed)
			await this.Destroy( count, token );
		else {
			_tokens.Adjust( token,-count);
			_tokens.Adjust(newToken,count);
		}
	}

	public async Task AdjustHealthOfAll( int delta ) {
		if( delta == 0 ) return;
		var orderedKeys = delta < 0 
			? Keys.OrderBy( x => x.FullHealth ).ToArray()
			: Keys.OrderByDescending( x => x.FullHealth ).ToArray();
		foreach(var t in orderedKeys)
			await AdjustHealthOf(t, delta, _tokens[t] );
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

		await _tokens.Remove( original, count, _destroyReason );
	}

	#endregion
}