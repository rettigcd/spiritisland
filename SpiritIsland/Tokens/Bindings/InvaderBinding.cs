namespace SpiritIsland;

public class InvaderBinding {

	#region constructor

	public InvaderBinding( SpaceState tokens, UnitOfWork actionScope) {
		this.Tokens = tokens;
		this.ActionScope = actionScope ?? throw new ArgumentNullException(nameof(actionScope));
	}

	#endregion

	public UnitOfWork ActionScope;
	public readonly SpaceState Tokens;

	#region Apply Damage To...

	/// <summary> Not Badland-aware </summary>
	public async Task ApplyDamageToEach( int individualDamage, params TokenClass[] generic ) {

		var invaders = Tokens.InvaderTokens()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && generic.Length>0)
			invaders = invaders.Where(t=>generic.Contains(t.Class)).ToArray();

		foreach(var invader in invaders)
			for(int num = Tokens[invader]; num>0; --num) // can't use while this[invader]>0 because BoD doesn't actually destroy them.
				await ApplyDamageTo1( individualDamage, invader );

	}

	/// <summary> Not Badland-aware </summary>
	/// <returns>(damage inflicted,damagedInvader)</returns>
	public async Task<(int,HealthToken)> ApplyDamageTo1( int availableDamage, HealthToken originalInvader ) {

		var damagedInvader = GetNewDamagedToken( originalInvader, availableDamage );

		await ReplaceOrDestroyOriginalToken( originalInvader, damagedInvader );

		int damageInflicted = originalInvader.RemainingHealth - damagedInvader.RemainingHealth;
		return (damageInflicted, damagedInvader);
	}

	async Task ReplaceOrDestroyOriginalToken( HealthToken invaderToken, HealthToken damagedInvader ) {
		if(!damagedInvader.IsDestroyed) {
			Tokens.Adjust( invaderToken, -1 );
			Tokens.Adjust( damagedInvader, 1 );
		} else
			await DestroyNTokens( 1, invaderToken );
	}

	protected virtual HealthToken GetNewDamagedToken( HealthToken invaderToken, int availableDamage ) => invaderToken.AddDamage( availableDamage );


	#endregion

	#region Destroy

	public async Task DestroyAll( params HealthTokenClass[] tokenClasses ) {
		var tokensToDestroy = Tokens.OfAnyHealthClass( tokenClasses ).ToArray();
		foreach(var token in tokensToDestroy)
			await token.DestroyAll( Tokens, ActionScope );
	}

	public async Task DestroyNOfAnyClass( int count, params HealthTokenClass[] generics ) {
		HealthToken[] invadersToDestroy;
		while(
			0 < count 
			&& (invadersToDestroy = Tokens.OfAnyHealthClass( generics ).ToArray()).Length > 0
		) {
			var invader = invadersToDestroy
				.OrderByDescending( x => x.FullHealth )
				.First();
			await DestroyNOfClass( 1, invader.Class );

			// next
			--count;
		}
	}


	// destroy CLASS
	public async Task<int> DestroyNOfClass( int countToDestroy, HealthTokenClass invaderClass ) {
		countToDestroy = Math.Min( countToDestroy, Tokens.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(0 < remaining) {
			var next = Tokens.OfClass( invaderClass ).Cast<HealthToken>()
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.FullDamage )
				.First();
			remaining -= await DestroyNTokens( remaining, next );
		}

		return countToDestroy;
	}

	// destroy TOKEN
	public virtual Task<int> DestroyNTokens( int countToDestroy, HealthToken invaderToDestroy ) {
		return invaderToDestroy.Destroy( Tokens, countToDestroy, ActionScope );
	}

	#endregion Destroy

	#region Remove

	/// <remarks>
	/// This is neither damage nor destroy.
	/// It is Game-Aware in that it understands non-strifed invaders are more dangerous than non-strifed, so it doesn't belong in the generic TokenDictionary class.
	/// However, it also does not require any input from a user, so it should not be on a TargetSpaceCtx.
	/// Sticking on InvaderGroup is the only place I can think to put it.
	/// Also, shouldn't be affected by Bringer overwriting 'Destroy' and 'Damage'
	/// </remarks>
	public async Task RemoveLeastDesirable( params TokenClass[] removables ) {
		if(Tokens.SumAny(removables) == 0) return;

		var invaderToRemove = Tokens.OfAnyClass( removables )
			.Cast<HealthToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();

		if(invaderToRemove != null)
			await Tokens.Remove( invaderToRemove, 1, ActionScope );
	}

	public Task Remove( Token token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, ActionScope, reason );

	#endregion

	#region UserSelected

	// This is needed when strifed invaders ravage OTHER tokens. Need to be able to exclude specific token
	// !!! ??? can this be merged with UserSelectedDamage above?
	public async Task<int> UserSelected_ApplyDamageToSpecificToken( int damage, Spirit damagePicker, HealthToken source, Func<HealthToken[]> allowedTypes ) {
		if(damage == 0) return 0;

		Token[] options;
		int damageInflicted = 0;
		while(0 < damage && (options = Tokens.Keys.OfType<HealthToken>().Intersect( allowedTypes() ).ToArray()).Length > 0) {
			var invaderToDamage = (HealthToken)await damagePicker.Gateway.Decision( Select.Invader.ForAggregateDamageFromSource( Tokens.Space, source, options, damage, Present.Always ) );
			if(invaderToDamage == null) break;
			await ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	public Task<int> UserSelectedDamage( int damage, Spirit damagePicker, params TokenClass[] allowedTypes ) {
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = new TokenClass[] { Invader.City, Invader.Town, Invader.Explorer };
		return UserSelectedDamage( damage, damagePicker, Present.Always, allowedTypes );
	}

	public Task<int> UserSelectedPartialDamage( int damage, Spirit damagePicker, params TokenClass[] allowedTypes ) {
		return UserSelectedDamage(damage,damagePicker, Present.Done, allowedTypes );
	}

	// This is the standard way of picking - by TokenClass
	// ??? !!! can we remove this and use DamageToSpecificTokens instead?
	async Task<int> UserSelectedDamage( int damage, Spirit damagePicker, Present present, params TokenClass[] allowedTypes ) {
		if(damage == 0) return 0;
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = new TokenClass[] { Invader.Explorer, Invader.Town, Invader.City };

		Token[] invaderTokens;
		int damageInflicted = 0;
		while(damage > 0 && (invaderTokens = Tokens.OfAnyClass( allowedTypes ).ToArray()).Length > 0) {
			var invaderToDamage = (HealthToken)await damagePicker.Gateway.Decision( Select.Invader.ForAggregateDamage( Tokens.Space, invaderTokens, damage, present ) );
			if(invaderToDamage==null) break;
			await ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	#endregion UserSelected

	/// <summary>
	/// Restores all of the tokens to their default / healthy state.
	/// </summary>
	static public void HealTokens( SpaceState counts ) {

		void RestoreAllToDefault( Token token ) {
			if(token is not HealthToken ht || ht.FullDamage == 0) return;
			int num = counts[token];
			counts.Adjust( ht.Healthy, num );
			counts.Adjust( token, -num );
		}

		void HealGroup( TokenClass group ) {
			foreach(var token in counts.OfClass( group ).ToArray())
				RestoreAllToDefault( token );
		}

		HealGroup( Invader.City );
		HealGroup( Invader.Town );
		HealGroup( TokenType.Dahan );
	}

}