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
	public async Task ApplyDamageToEach( int individualDamage, params IEntityClass[] generic ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		var invaders = Tokens.InvaderTokens()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && 0<generic.Length)
			invaders = invaders.Where(t=>generic.Contains(t.Class)).ToArray();

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

		var tokensToDestroy = Tokens.OfAnyHumanClass( tokenClasses ).ToArray();
		foreach(var token in tokensToDestroy)
			await token.DestroyAll( Tokens );
	}

	public async Task DestroyNOfAnyClass( int count, params HumanTokenClass[] generics ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		HumanToken[] invadersToDestroy;
		while(
			0 < count 
			&& (invadersToDestroy = Tokens.OfAnyHumanClass( generics ).ToArray()).Length > 0
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
	public async Task<int> DestroyNOfClass( int countToDestroy, HumanTokenClass invaderClass ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return 0;

		countToDestroy = Math.Min( countToDestroy, Tokens.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(0 < remaining) {
			var next = Tokens.OfHumanClass( invaderClass )
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.FullDamage )
				.First();
			remaining -= await DestroyNTokens( next, remaining );
		}

		return countToDestroy;
	}

	// destroy TOKEN
	public async Task<int> DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		return Tokens.ModsOfType<IStopInvaderDamage>().Any() ? 0 
			: await invaderToDestroy.Destroy( Tokens, countToDestroy );
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
	public async Task RemoveLeastDesirable( RemoveReason reason = RemoveReason.Removed, params IEntityClass[] removables ) {
		if(Tokens.SumAny(removables) == 0) return;

		var invaderToRemove = Tokens.OfAnyClass( removables )
			.Cast<HumanToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();

		if(invaderToRemove != null)
			await Tokens.Remove( invaderToRemove, 1, reason );
	}

	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, reason );

	#endregion

	#region UserSelected

	// This is needed when strifed invaders ravage OTHER tokens. Need to be able to exclude specific token
	public async Task<int> UserSelected_ApplyDamageToSpecificToken( int damage, Spirit damagePicker, HumanToken source, Func<HumanToken[]> allowedTypes ) {
		if(damage == 0) return 0;

		IToken[] options;
		int damageInflicted = 0;
		while(0 < damage && (options = Tokens.OfTypeHuman().Intersect( allowedTypes() ).ToArray()).Length > 0) {
			var st = await damagePicker.Gateway.Decision( Select.Invader.ForAggregateDamageFromSource( Tokens.Space, source, options, damage, Present.Always ) );
			if( st == null) break;
			var invaderToDamage = st.Token.AsHuman();
			await ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	public Task<int> UserSelectedDamage( Spirit damagePicker, int damage, params IEntityClass[] allowedTypes ) {
		return UserSelectedDamage_Internal( damage, damagePicker, Present.Always, allowedTypes );
	}

	public Task<int> UserSelectedPartialDamage( int damage, Spirit damagePicker, params IEntityClass[] allowedTypes ) {
		return UserSelectedDamage_Internal(damage,damagePicker, Present.Done, allowedTypes );
	}

	// This is the standard way of picking - by TokenClass
	async Task<int> UserSelectedDamage_Internal( int damage, Spirit damagePicker, Present present, params IEntityClass[] allowedTypes ) {
		if(damage == 0) return 0;
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = Human.Invader;

		IToken[] invaderTokens;
		int damageInflicted = 0;
		while(damage > 0 && (invaderTokens = Tokens.OfAnyClass( allowedTypes ).Cast<IToken>().ToArray()).Length > 0) {
			var st = await damagePicker.Gateway.Decision( Select.Invader.ForAggregateDamage( Tokens.Space, invaderTokens, damage, present ) );
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
			foreach(var token in counts.OfHumanClass( group ).ToArray())
				RestoreAllToDefault( token );
		}

		HealGroup( Human.City );
		HealGroup( Human.Town );
		HealGroup( Human.Dahan );
	}

}