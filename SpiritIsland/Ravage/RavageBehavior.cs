namespace SpiritIsland;

/// <summary>
/// Configures Dahan and Invader behavior on a per-space bases.
/// </summary>
public class RavageBehavior {

	// Order / Who is damaged
	public Func<RavageBehavior, RavageData, Task> RavageSequence = RavageSequence_Default;
	public Func<RavageBehavior, CountDictionary<HealthToken>, SpaceState, int> GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers_Default;
	public Func<RavageBehavior, RavageData, int, Task> DamageDefenders = DamageDefenders_Default;
	public CountDictionary<Token> NotParticipating { get; set; } = new CountDictionary<Token>();
	public Func<HealthToken, bool> IsAttacker { get; set; } = null;
	public Func<HealthToken, bool> IsDefender { get; set; } = null;
	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	public RavageBehavior Clone() {
		return new RavageBehavior {
			RavageSequence = RavageSequence,
			GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers,
			DamageDefenders = DamageDefenders,
			NotParticipating = NotParticipating,
			IsAttacker = IsAttacker,
			IsDefender = IsDefender,
			AttackersDefend = AttackersDefend
		};
	}

	public async Task Exec( SpaceState tokens, GameState gameState ) {
		RavageData data = new RavageData( tokens, gameState );

		// Check for Stoppers
		var stoppers = data.Tokens.Keys.OfType<ISkipRavages>()
			.OrderBy( s => s.Cost )
			.ToArray();

		foreach(var stopper in stoppers)
			if(await stopper.Skip( data.GameState, data.Tokens ))
				return; // baby steps, don't break tests.  Eventually we want: $"stopped by {stopper.Text}";

		data.ActionScope = data.GameState.StartAction( ActionCategory.Invader );
		data.InvaderBinding = new InvaderBinding( data.Tokens, data.ActionScope );

		try {
			// Record starting state
			data.Result.startingAttackers = RavageBehavior.GetAttackers( this, data );
			data.Result.startingDefenders = RavageBehavior.GetDefenders( this, data );

			await RavageSequence( this, data );

			data.GameState.Log( new RavageEntry( data.Result ) );
		}
		finally {
			if(data.ActionScope != null) {
				await data.ActionScope.DisposeAsync();
				data.ActionScope = null;
				data.InvaderBinding = null;
			}
		}
	}

	static async Task RavageSequence_Default( RavageBehavior behavior, RavageData data ) {
		// Default Ravage Sequence
		int damageInflictedByAttackers = GetDamageInflictedByAttackers( behavior, data );
		await behavior.DamageDefenders( behavior, data, damageInflictedByAttackers );
		int damageFromDefenders = RavageBehavior.GetDamageInflictedByDefenders( behavior,data );
		await DamageAttackers( data, damageFromDefenders );

		// worry about this last - why?, just because
		await DamageLand( data, damageInflictedByAttackers );
	}

	/// <returns># of dahan killed/destroyed</returns>
	static public async Task DamageDefenders_Default( RavageBehavior behavior, RavageData data, int damageInflictedFromAttackers ) {

		// Start with Damage from attackers
		if(damageInflictedFromAttackers == 0) return;

		data.Result.defenderDamageFromAttackers = damageInflictedFromAttackers; // Defend points already applied.

		// Add Badlands damage
		data.Result.defenderDamageFromBadlands = data.BadLandsCount;
		int damageToApply = damageInflictedFromAttackers + data.BadLandsCount; // to defenders

		var defenders = data.Result.startingDefenders.Clone();

		// Damage helping Invaders
		// When damaging defenders, it is ok to damage the explorers first.
		// https://querki.net/u/darker/spirit-island-faq/#!Voice+of+Command 
		var participatingExplorers = defenders.Keys
			.Where( k => k.Class.Category == TokenCategory.Invader ) //! all defending invaders, even dreaming/frozen invaders
			.OfType<HealthToken>()
			.OrderByDescending( x => x.RemainingHealth )
			.ThenBy( x => x.StrifeCount ) // non strifed first
			.ToArray();
		if(participatingExplorers.Length > 0) {
			foreach(var token in participatingExplorers) {
				int tokensToDestroy = Math.Min( data.Result.startingDefenders[token], damageToApply / token.RemainingHealth );
				// destroy real tokens
				await data.InvaderBinding.DestroyNTokens( tokensToDestroy, token );
				// update our defenders count
				defenders[token] -= tokensToDestroy;
				damageToApply -= tokensToDestroy * token.RemainingHealth;
			}
		}

		// !! if we had cities / towns helping with defend, we would want to apply partial damage to them here.

		// Dahan
		var damagableDahan = defenders.Keys
			.Cast<HealthToken>()
			.Where( k => k.Class == TokenType.Dahan )  // Normal only, filters out frozen/sleeping
			.OrderBy( t => t.RemainingHealth ) // kill damaged dahan first
			.ToArray();

		var dahan = new DahanGroupBinding( data.Tokens, data.ActionScope );

		foreach(var dahanToken in damagableDahan) {

			// 1st - Destroy weekest dahan first
			int tokensToDestroy = Math.Min(
				data.Result.startingDefenders[dahanToken],
				damageToApply / dahanToken.RemainingHealth // rounds down
			);
			if(0 < tokensToDestroy) {

				var removed = await dahan.DestroyToken( tokensToDestroy, dahanToken );

				// use up damage
				damageToApply -= tokensToDestroy * dahanToken.RemainingHealth;

				data.Result.dahanDestroyed += removed;
				defenders[dahanToken] -= removed;
			}
			// damage real tokens

			// 2nd - if we no longer have enougth to destroy this token, apply damage all the damage that remains
			if(0 < defenders[dahanToken] && 0 < damageToApply) {

				await dahan.ApplyDamage_Efficiently( damageToApply, dahanToken ); // this will never destroy token

				// update our defenders count
				++defenders[dahanToken.AddDamage( damageToApply )];
				--defenders[dahanToken];
				damageToApply = 0;
			}

		}

	}

	/// <summary> Defend has already been applied. </summary>
	static public int GetDamageInflictedByAttackers( RavageBehavior behavior, RavageData data ) {

		// CurrentAttackers
		int rawDamageFromAttackers = behavior.GetDamageFromParticipatingAttackers( behavior, data.Result.startingAttackers, data.Tokens );

		data.CurrentAttackers = FromEachStrifed_RemoveOneStrife( data ); // does not change start state, modifies gs.Tokens[...] instead

		// Defend
		data.Result.defend = data.Tokens.Defend.Count;
		int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - data.Result.defend, 0 );

		return damageInflictedFromAttackers;
	}

	/// <returns>New attacker finvaders</returns>
	static CountDictionary<HealthToken> FromEachStrifed_RemoveOneStrife( RavageData ra ) {
		CountDictionary<HealthToken> participatingInvaders = ra.Result.startingAttackers;

		var newAttackers = participatingInvaders.Clone();

		var strifed = participatingInvaders.Keys
			.OfType<HealthToken>()
			.Where( x => x.StrifeCount > 0 )
			.OrderBy( x => x.StrifeCount ) // smallest first
			.ToArray();
		foreach(var orig in strifed) {
			// update tracking counts
			int count = ra.Tokens[orig];
			newAttackers[orig] -= count;
			newAttackers[orig.AddStrife( -1 )] += count;

			// update real tokens
			ra.Tokens.RemoveStrife( orig, ra.Tokens[orig] );
		}

		return newAttackers;
	}

	static public int GetDamageInflictedByDefenders( RavageBehavior behavior, RavageData data ) {
		CountDictionary<HealthToken> participants = GetDefenders( behavior,data);
		int damageFromDefenders = participants.Sum( pair => data.Tokens.AttackDamageFrom1( pair.Key ) * pair.Value );
		return Math.Max( 0, damageFromDefenders - behavior.AttackersDefend );
	}

	static int GetDamageFromParticipatingAttackers_Default( RavageBehavior cfg, CountDictionary<HealthToken> participatingAttacker, SpaceState tokens ) {

		return participatingAttacker.Keys
			.OfType<HealthToken>()
			.Where( x => x.StrifeCount == 0 )
			.Select( attacker => tokens.AttackDamageFrom1( attacker ) * participatingAttacker[attacker] ).Sum();
	}

	/// <returns>(city-dead,town-dead,explorer-dead)</returns>
	static public async Task DamageAttackers( RavageData ra, int damageFromDefenders ) {
		ra.Result.attackerDamageFromDefenders = damageFromDefenders;
		if(damageFromDefenders == 0) return;

		ra.Result.attackerDamageFromBadlands = ra.BadLandsCount;
		int remainingDamageToApply = damageFromDefenders + ra.BadLandsCount;

		// ! must use current Attackers counts, because some have lost their strife so tokens are different than when they started.
		var remaningAttackers = ra.CurrentAttackers.Clone();

		while(remainingDamageToApply > 0 && remaningAttackers.Any()) {
			HealthToken attackerToDamage = PickSmartInvaderToDamage( remaningAttackers, remainingDamageToApply );

			// Apply real damage
			var (damageInflicted, _) = await ra.InvaderBinding.ApplyDamageTo1( remainingDamageToApply, attackerToDamage );
			remainingDamageToApply -= damageInflicted;

			// Apply tracking damage
			--remaningAttackers[attackerToDamage];
			var damagedAttacker = attackerToDamage.AddDamage( damageInflicted );
			if(damagedAttacker.RemainingHealth > 0) ++remaningAttackers[damagedAttacker];

		}
		ra.Result.endingAttackers = remaningAttackers;
	}

	static public async Task DamageLand( RavageData ra, int damageInflictedFromInvaders ) {
		await ra.GameState.DamageLandFromRavage( ra.InvaderBinding.Tokens.Space, damageInflictedFromInvaders, ra.ActionScope );
	}

	#region Static Smart Damage to Invaders

	public static HealthToken PickSmartInvaderToDamage( CountDictionary<HealthToken> participatingInvaders, int availableDamage ) {
		return PickItemToKill( participatingInvaders.Keys, availableDamage )
			?? PickItemToDamage( participatingInvaders.Keys );
	}

	public static HealthToken PickItemToKill( IEnumerable<HealthToken> candidates, int availableDamage ) {
		return candidates
			.Where( specific => specific.RemainingHealth <= availableDamage ) // can be killed
			.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
			.ThenBy( k => k.RemainingHealth ) // and most damaged.
			.FirstOrDefault();
	}

	public static HealthToken PickItemToDamage( IEnumerable<HealthToken> candidates ) {
		return candidates
			.OrderBy( i => i.RemainingHealth ) // closest to dead
			.ThenByDescending( i => i.FullHealth ) // biggest impact
			.First();
	}

	#endregion

	public static CountDictionary<HealthToken> GetAttackers( RavageBehavior behavior, RavageData data ) 
		=> GetParticipantCounts( behavior, data, behavior.IsAttacker ?? IsInvader );

	static public CountDictionary<HealthToken> GetDefenders( RavageBehavior behavior, RavageData data ) 
		=> GetParticipantCounts( behavior, data, behavior.IsDefender ?? IsDahan );

	static bool IsInvader( Token token ) => token.Class.Category == TokenCategory.Invader;
	static bool IsDahan( Token token ) => token.Class.Category == TokenCategory.Dahan; // all Dahan, including Frozen

	static CountDictionary<HealthToken> GetParticipantCounts( RavageBehavior cfg, RavageData ra, Func<HealthToken, bool> filter ) {
		var participants = new CountDictionary<HealthToken>();
		foreach(var token in ra.Tokens.Keys.OfType<HealthToken>().Where( filter ))
			participants[token] = Math.Max( 0, ra.Tokens[token] - cfg.NotParticipating[token] );
		return participants;
	}



}