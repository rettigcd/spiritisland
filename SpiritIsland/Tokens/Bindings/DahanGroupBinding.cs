namespace SpiritIsland;

public class DahanGroupBinding {

	public bool Frozen { get; set; }

	readonly TokenCountDictionary tokens;
	readonly HealthTokenClass tokenGroup;

	public DahanGroupBinding( TokenCountDictionary tokens ) {
		this.tokens = tokens;
		this.tokenGroup = TokenType.Dahan;
	}

	public IEnumerable<HealthToken> Keys => tokens.OfType(tokenGroup).Cast<HealthToken>();

	public bool Any => Count > 0;

	public int Count => tokens.Sum(tokenGroup);

	public static implicit operator int( DahanGroupBinding b ) => b.Count;

	/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
	public Task Add( int count, AddReason reason = AddReason.Added ) {
		return tokens.AddDefault(TokenType.Dahan,count, reason );
	}

	public void Init(int count ) => tokens.InitDefault(TokenType.Dahan, count );

	public void Adjust(Token token, int delta ) => tokens.Adjust(token,delta);

	public void Init(Token token, int count ) => tokens.Init(token,count);

	/// <summary> Returns the Token removed </summary>
	public async Task<Token> Remove1( RemoveReason reason ) {
		if(Frozen) return null;

		var toRemove = Keys.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		if( toRemove != null)
			await tokens.Remove( toRemove, 1, reason );
		return toRemove;
	}

	public async Task<bool> Remove1( Token desiredToken, RemoveReason reason ) {
		if( !Frozen && 0<tokens[desiredToken] ){ 
			await tokens.Remove(desiredToken,1, reason );
			return true;
		}

		return false; // unable to remove desired token
	}

	public void Drown() {

		// !!! need to change this to async so we publish the .Destroy event and UI will update.

		if(Frozen) return;
		foreach( var token in Keys.ToArray() )
			tokens.Init(token,0);
	}

	public async Task AdjustHealthOf( HealthToken token, int delta, int count ) {
		count = Math.Min( tokens[token], count );
		if( count == 0 ) return;

		var newToken = token.AddHealth( delta );
		if( newToken.IsDestroyed)
			await this.Destroy( count, token, Cause.None ); // !!! Cause is wrong.
		else {
			tokens.Adjust( token,-count);
			tokens.Adjust(newToken,count);
		}
	}

	public async Task AdjustHealthOfAll( int delta ) {
		if( delta == 0 ) return;
		var orderedKeys = delta < 0 
			? Keys.OrderBy( x => x.FullHealth ).ToArray()
			: Keys.OrderByDescending( x => x.FullHealth ).ToArray();
		foreach(var t in orderedKeys)
			await AdjustHealthOf(t, delta, tokens[t] );
	}


	#region Damage

	public async Task Apply1DamageToAll( Cause cause ) { // Called By Power (i.e. not invaders)
		if(Frozen) return;

		var before = Keys.OrderByDescending(x=>x.Damage).ToArray(); // most damaged to least damaged so they don't cascade
		foreach(var token in before) {
			int origCount = tokens[token];
			var newToken = token.AddDamage( 1 );
			if( newToken.IsDestroyed) {
				// Apply 1 damage to all
				tokens.Init( token, 0 );
				tokens.Adjust( newToken, origCount );
			} else
				// or destory
				await Destroy( origCount, token, cause );
		}

	}

	public async Task ApplyDamage( int damageToDahan, Cause cause ) {
		if(Frozen) return;

		var before = Keys.OrderBy(x=>x.RemainingHealth).ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			if(token.RemainingHealth <= damageToDahan ) {
				int destroyed = damageToDahan / token.RemainingHealth;
				await Destroy(destroyed,token,cause);
				damageToDahan -= destroyed * token.RemainingHealth;
			}
			// if we can further damage the most damage
			if( damageToDahan == 0 )
				break;
			else if(0 < tokens[token]) {
				tokens.Adjust( token, -1 );
				tokens.Adjust( token.AddDamage( damageToDahan ), 1 );
				break;
			}
		}

		//// Destroy injured first
		//int damagedToDestroy = System.Math.Min( this[1], damageToDahan );
		//if( 0 < damagedToDestroy ) {
		//	await Destroy(damagedToDestroy,1, cause);
		//	damageToDahan -= damagedToDestroy;
		//}

		//// Destroy healthy next
		//int healthyToDestroy = System.Math.Min( this[2], damagedToDestroy/2 );
		//if( 0 < healthyToDestroy ) {
		//	await Destroy(healthyToDestroy,2,cause);
		//	damageToDahan -= healthyToDestroy * 2;
		//}

		//// Injure remaining
		//if( damageToDahan == 1 && 0 < this[2]) {
		//	this.InitDamaged( 1 );
		//	this.Init( this[2]-1 );
		//}

	}

	#endregion

	#region Destroy

	public async Task Destroy( int countToDestroy, Cause cause ) {
		if(Frozen) return;

		var before = Keys.OrderBy( x => x.RemainingHealth ).ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyed = Math.Min(countToDestroy, tokens[token]);
			await Destroy( destroyed, token, cause );
			countToDestroy -= destroyed;
		}
	}

	public async Task Destroy( int count, HealthToken original, Cause cause ) {
		if(Frozen) return;

		var reason = cause == Cause.Invaders
			? RemoveReason.DestroyedInBattle
			: RemoveReason.Destroyed;

		await tokens.Remove( original, count, reason );
	}

	#endregion
}