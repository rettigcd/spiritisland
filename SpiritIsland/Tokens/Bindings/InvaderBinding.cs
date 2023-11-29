namespace SpiritIsland;

public sealed class InvaderBinding {

	#region constructor

	public InvaderBinding( SpaceState tokens ) {
		Tokens = tokens;
	}

	#endregion

	public readonly SpaceState Tokens;

	#region Apply Damage To...

	/// <summary> Not Badland-aware </summary>
	public async Task ApplyDamageToEach( int individualDamage, params ITokenClass[] generic ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		var invaders = Tokens.InvaderTokens()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && 0<generic.Length)
			invaders = invaders.Where(t=>generic.Contains(t.HumanClass)).ToArray();

		foreach(var invader in invaders)
			for(int num = Tokens[invader]; num>0; --num) // can't use while this[invader]>0 because BoD doesn't actually destroy them.
				await ApplyDamageTo1( individualDamage, invader );

	}

	/// <summary> Not Badland-aware </summary>
	/// <returns>(damage inflicted,damagedInvader)</returns>
	public async Task<(int,HumanToken)> ApplyDamageTo1( int availableDamage, HumanToken originalInvader ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return (0,originalInvader);

		var damagedInvader = Tokens.GetNewDamagedToken( originalInvader, availableDamage );

		if(!damagedInvader.IsDestroyed) {
			Tokens.Adjust( originalInvader, -1 );
			Tokens.Adjust( damagedInvader, 1 );
			InvaderDamaged?.Invoke( originalInvader );
		} else
			await DestroyNTokens( originalInvader, 1 );

		int damageInflicted = originalInvader.RemainingHealth - damagedInvader.RemainingHealth;
		return (damageInflicted, damagedInvader);
	}
	// The invader Before it was damaged.
	public event Action<HumanToken> InvaderDamaged;

	#endregion

	#region Destroy

	public async Task DestroyAll( params HumanTokenClass[] tokenClasses ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		var tokensToDestroy = Tokens.HumanOfAnyTag( tokenClasses ).ToArray();
		foreach(var token in tokensToDestroy)
			await token.DestroyAll( Tokens );
	}

	public async Task DestroyNOfAnyClass( int count, params HumanTokenClass[] generics ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		HumanToken[] invadersToDestroy;
		while(
			0 < count 
			&& (invadersToDestroy = Tokens.HumanOfAnyTag( generics ).ToArray()).Length > 0
		) {
			var invader = invadersToDestroy
				.OrderByDescending( x => x.FullHealth )
				.First();
			await DestroyNOfClass( 1, invader.HumanClass );

			// next
			--count;
		}
	}


	// destroy CLASS
	public async Task<int> DestroyNOfClass( int countToDestroy, HumanTokenClass invaderClass ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return 0;

		countToDestroy = Math.Min( countToDestroy, Tokens.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(0 < remaining) {
			var next = Tokens.HumanOfTag( invaderClass )
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.FullDamage )
				.First();
			int countOfTypeToDestroy = Math.Min( remaining, Tokens[next]);
			await DestroyNTokens( next, countOfTypeToDestroy );
			remaining -= countOfTypeToDestroy;
		}

		return countToDestroy;
	}

	// destroy TOKEN
	public async Task DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;
		await invaderToDestroy.Destroy( Tokens, countToDestroy );
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
	public async Task RemoveLeastDesirable( RemoveReason reason = RemoveReason.Removed, params ITokenClass[] removables ) {
		if(Tokens.SumAny(removables) == 0) return;

		var invaderToRemove = Tokens.OfAnyTag( removables )
			.Cast<HumanToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();

		if(invaderToRemove != null)
			await Tokens.RemoveAsync( invaderToRemove, 1, reason );
	}

	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.RemoveAsync( token, count, reason );

	#endregion

	#region UserSelected

	// This is needed when strifed invaders ravage OTHER tokens. Need to be able to exclude specific token
	public async Task<int> UserSelected_ApplyDamageToSpecificToken( int damage, Spirit damagePicker, HumanToken source, Func<HumanToken[]> allowedTypes ) {
		if(damage == 0) return 0;

		IToken[] options;
		int damageInflicted = 0;
		while(0 < damage && (options = Tokens.Humans().Intersect( allowedTypes() ).ToArray()).Length > 0) {
			var st = await damagePicker.Select( An.Invader.ForAggregateDamageFromSource( Tokens.Space, source, options, damage, Present.Always ) );
			if( st == null) break;
			var invaderToDamage = st.Token.AsHuman();
			await ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	public Task<int> UserSelectedDamageAsync( Spirit damagePicker, int damage, params ITokenClass[] allowedTypes ) {
		if(allowedTypes.Length == 0) allowedTypes = Human.Invader;
		SourceSelector selector = new SourceSelector(Tokens).AddAll(allowedTypes);
		return UserSelectedDamageAsync( damagePicker, damage, selector, Present.Always );
	}

	// This is the standard way of picking - by TokenClass
	public async Task<int> UserSelectedDamageAsync( Spirit damagePicker, int damage, SourceSelector selector, Present present ) {
		if(damage == 0) return 0;

		selector
			.ConfigPrompt(_ => $"Damage ({damage} remaining)")
			.FilterSpaceToken(_ => 0 < damage);

		int damageInflicted = 0;
		while( true) {
			var st = await selector.GetSource( damagePicker, "", present ); 
			if(st==null) break;
			var invaderToDamage = st.Token.AsHuman();
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

		void RestoreAllToDefault( ISpaceEntity token ) {
			if(token is not HumanToken ht || ht.FullDamage == 0) return;
			int num = counts[token];
			counts.Adjust( ht.Healthy, num );
			counts.Adjust( token, -num );
		}

		void HealGroup( HumanTokenClass group ) {
			foreach(var token in counts.HumanOfTag( group ).ToArray())
				RestoreAllToDefault( token );
		}

		HealGroup( Human.City );
		HealGroup( Human.Town );
		HealGroup( Human.Dahan );
	}

}