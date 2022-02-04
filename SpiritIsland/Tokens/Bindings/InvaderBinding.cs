namespace SpiritIsland;

public class InvaderBinding {

	#region constructor

	public InvaderBinding( TokenCountDictionary tokens, DestroyInvaderStrategy destroyStrategy, IDamageApplier customeDamageApplicationStrategy = null) {
		this.Tokens = tokens;
		this.damageApplicationStrategy = customeDamageApplicationStrategy ?? DefaultDamageApplicationStrategy;
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
			.OrderBy(x=>x.Health) // do damaged first to clear them out
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
	public async Task<(int,Token)> ApplyDamageTo1( int availableDamage, Token invaderToken, bool fromRavage = false ) {

		Token damagedInvader = damageApplicationStrategy.ApplyDamage( Tokens, availableDamage, invaderToken );

		if(0 == damagedInvader.Health)
			await DestroyStrategy.OnInvaderDestroyed( Space, damagedInvader, fromRavage );

		int damageInflicted = invaderToken.Health - damagedInvader.Health;
		return (damageInflicted,damagedInvader); // damage inflicted
	}

	readonly IDamageApplier damageApplicationStrategy;

	#endregion

	#region Destroy

	public async Task<int> Destroy( int countToDestroy, TokenClass tokenClass ) {
		if(countToDestroy == 0) return 0;
		Token[] invaderTypesToDestroy = Tokens.Invaders()
			.Where( x=> x.Class == tokenClass )
			.OrderByDescending( x => x.Health ) // kill healthiest first
			.ToArray();

		int totalDestoyed = 0;
		foreach(var specificInvader in invaderTypesToDestroy) {
			while(countToDestroy > 0 && this[specificInvader] > 0) {
				await ApplyDamageTo1( specificInvader.Health, specificInvader );
				++totalDestoyed;
				--countToDestroy;
			}
		}
		return totalDestoyed;
	}

	public async Task<int> Destroy( int countToDestroy, Token specificInvader ) {
		if(countToDestroy == 0) return 0;
		int numToDestroy = Math.Min(countToDestroy, this[specificInvader] );
		for(int i = 0; i < numToDestroy; ++i)
			await ApplyDamageTo1( specificInvader.Health, specificInvader );
		return numToDestroy;
	}

	public async Task DestroyAny( int count, params TokenClass[] generics ) {
		// !! this could be cleaned up
		Token[] invadersToDestroy = Tokens.OfAnyType( generics );
		while(count > 0 && invadersToDestroy.Length > 0) {
			var invader = invadersToDestroy
				.OrderByDescending(x=>x.FullHealth)
				// .ThenByDescending(x=>x.Health) assume this line is in Destroy(...)
				.First();
			await Destroy( 1, invader.Class );

			// next
			invadersToDestroy = Tokens.OfAnyType( generics );
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
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.Strife() )  // un-strifed first
			.ThenByDescending( g => g.Health )
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
			if(token == token.Healthy) return; // already at default/healthy
			int num = counts[token];
			counts.Adjust( token.Healthy, num );
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

	public async Task UserSelectedDamage( int damage, Spirit damagePicker, params TokenClass[] allowedTypes ) {
		if(damage == 0) return;
		if(allowedTypes == null || allowedTypes.Length == 0)
			allowedTypes = new TokenClass[] { Invader.Explorer, Invader.Town, Invader.City };

		Token[] invaderTokens;
		while(damage>0 && (invaderTokens=Tokens.OfAnyType(allowedTypes).ToArray()).Length > 0) {
			var invaderToDamage = await damagePicker.Action.Decision( Select.Invader.ForAggregateDamage(Space, invaderTokens, damage ) );
			await ApplyDamageTo1(1,invaderToDamage);
			damage--;
		}
	}

	public readonly DestroyInvaderStrategy DestroyStrategy;
	public readonly TokenCountDictionary Tokens;

	static readonly DefaultDamageApplier DefaultDamageApplicationStrategy = new DefaultDamageApplier(); // changed By Flames Fury

}