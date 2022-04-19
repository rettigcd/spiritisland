﻿namespace SpiritIsland;

public class InvaderBinding {

	#region constructor

	public InvaderBinding( TokenCountDictionary tokens, DestroyInvaderStrategy destroyStrategy) {
		this.Tokens = tokens;
		this.DestroyStrategy = destroyStrategy;
	}

	#endregion

	public Space Space => Tokens.Space;

	#region Read-only
	public int this[Token specific] => Tokens[specific];

	public int DamageInflictedByInvaders => Tokens.Invaders().Select( invader => invader.FullHealth * this[invader] ).Sum();

	#endregion

	#region Damage

	/// <summary> Not Badland-aware </summary>
	public async Task ApplyDamageToEach( int individualDamage, params TokenClass[] generic ) {

		var invaders = Tokens.Invaders()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && generic.Length>0)
			invaders = invaders.Where(t=>generic.Contains(t.Class)).ToArray();

		foreach(var invader in invaders)
			while(this[invader] > 0)
				await ApplyDamageTo1( individualDamage, invader );

	}

	/// <summary> Not Badland-aware </summary>
	/// <returns>(damage inflicted,damagedInvader)</returns>
	public async Task<(int,Token)> ApplyDamageTo1( int availableDamage, HealthToken invaderToken, bool fromRavage = false ) { // !! change Token to HealthToken

		var damagedInvader = invaderToken.AddDamage( availableDamage );
		if(!damagedInvader.IsDestroyed) {
			Tokens.Adjust( invaderToken, -1 );
			Tokens.Adjust( damagedInvader, 1 );
		} else 
			await DestroyStrategy.OnInvaderDestroyed( Space, invaderToken, fromRavage );

		int damageInflicted = invaderToken.RemainingHealth - damagedInvader.RemainingHealth;
		return (damageInflicted,damagedInvader); // damage inflicted
	}

	#endregion

	#region Destroy

	public async Task<int> Destroy( int countToDestroy, HealthTokenClass invaderClass ) {
		countToDestroy = Math.Min( countToDestroy, Tokens.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(remaining > 0) {
			var next = Tokens.OfType( invaderClass ).Cast<HealthToken>()
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.Damage )
				.First();
			remaining -= await Destroy( remaining, next );
		}

		return countToDestroy;
	}

	public async Task<int> Destroy( int countToDestroy, HealthToken invaderToDestroy ) {
		int numToDestroy = Math.Min(countToDestroy, this[invaderToDestroy] );
		for(int i = 0; i < numToDestroy; ++i) {
			await DestroyStrategy.OnInvaderDestroyed( Space, invaderToDestroy, false );
		}
		return numToDestroy;
	}

	public async Task DestroyAny( int count, params HealthTokenClass[] generics ) {
		// !! this could be cleaned up
		HealthToken[] invadersToDestroy = Tokens.OfAnyType( generics ).Cast<HealthToken>().ToArray();
		while(count > 0 && invadersToDestroy.Length > 0) {
			var invader = invadersToDestroy
				.OrderByDescending(x=>x.FullHealth)
				// .ThenByDescending(x=>x.Health) assume this line is in Destroy(...)
				.First();
			await Destroy( 1, invader.Class );

			// next
			invadersToDestroy = Tokens.OfAnyType( generics ).Cast<HealthToken>().ToArray();
			--count;
		}
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
	public async Task Remove( params TokenClass[] removables ) {
		if(Tokens.SumAny(removables) == 0) return;

		var invaderToRemove = Tokens.OfAnyType( removables )
			.Cast<HealthToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();

		if(invaderToRemove != null)
			await Tokens.Remove( invaderToRemove, 1 );
	}

	public Task Remove( Token token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, reason );

	#endregion

	/// <summary>
	/// Restores all of the tokens to their default / healthy state.
	/// </summary>
	static public void HealTokens( TokenCountDictionary counts ) {

		void RestoreAllToDefault( Token token ) {
			if(token is not HealthToken ht || ht.Damage == 0) return;
			int num = counts[token];
			counts.Adjust( ht.Healthy, num );
			counts.Adjust( token, -num );
		}

		void HealGroup( TokenClass group ) {
			foreach(var token in counts.OfType(group).ToArray())
				RestoreAllToDefault(token);
		}

		HealGroup( Invader.City );
		HealGroup( Invader.Town );
		HealGroup( TokenType.Dahan );
	}

	public Task<int> UserSelectedDamage( int damage, Spirit damagePicker, params TokenClass[] allowedTypes ) {
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = new TokenClass[] { Invader.City, Invader.Town, Invader.Explorer };
		return UserSelectedDamage( damage, damagePicker, Present.Always, allowedTypes );
	}

	public Task<int> UserSelectedPartialDamage( int damage, Spirit damagePicker, params TokenClass[] allowedTypes ) {
		return UserSelectedDamage(damage,damagePicker, Present.Done, allowedTypes );
	}


	async Task<int> UserSelectedDamage( int damage, Spirit damagePicker, Present present, params TokenClass[] allowedTypes ) {
		if(damage == 0) return 0;
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = new TokenClass[] { Invader.Explorer, Invader.Town, Invader.City };

		Token[] invaderTokens;
		int damageInflicted = 0;
		while(damage > 0 && (invaderTokens = Tokens.OfAnyType( allowedTypes ).ToArray()).Length > 0) {
			var invaderToDamage = (HealthToken)await damagePicker.Action.Decision( Select.Invader.ForAggregateDamage( Space, invaderTokens, damage, present ) );
			if(invaderToDamage==null) break;
			await ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	public readonly DestroyInvaderStrategy DestroyStrategy;
	public readonly TokenCountDictionary Tokens;

}