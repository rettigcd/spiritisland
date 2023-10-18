namespace SpiritIsland;

/// <summary>
/// Configures Dahan and Invader behavior on a per-space bases.
/// </summary>
public class RavageBehavior : ISpaceEntity, IEndWhenTimePasses {

	public static RavageBehavior DefaultBehavior => GameState.Current.DefaultRavageBehavior;

	public static readonly IEntityClass Class = new ActionModTokenClass("RavageBehavior");
	IEntityClass ISpaceEntity.Class => Class;

	// Order / Who is damaged
	public Func<RavageBehavior, RavageData, Task> RavageSequence = RavageSequence_Default;

	// Gets the Aggregate damage from attackers. (default action does this by calling AttackDamageFrom1)
	// !! This can be removed if we slap a Town-Tracking token on every space Which updates a 'NeighboringTownsToken' that does damage.
	public Func<RavageBehavior, CountDictionary<HumanToken>, SpaceState, int> GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers_Default;

	public CountDictionary<ISpaceEntity> NotParticipating { get; set; } = new CountDictionary<ISpaceEntity>();

	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	public RavageBehavior Clone() {
		return new RavageBehavior {
			RavageSequence = RavageSequence,
			GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers,
			NotParticipating = NotParticipating,
			AttackersDefend = AttackersDefend
		};
	}

	/// <summary> Executes up to 1 potential Ravage </summary>
	public async Task Exec( SpaceState tokens ) {
		RavageData data = new RavageData( tokens );

		var scope = await ActionScope.Start( ActionCategory.Invader ); // start scope before Stoppers run

		// Check for Stoppers
		var stoppers = data.Tokens.ModsOfType<ISkipRavages>()
			.OrderBy( s => s.Cost )
			.ToArray();

		foreach(ISkipRavages stopper in stoppers)
			if(await stopper.Skip( data.Tokens )) {
				// baby steps, don't break tests.  Eventually we want: $"stopped by {stopper.SourceLabel}";
				return; 
			}

		data.InvaderBinding = data.Tokens.Invaders;

		try {
			// Record starting state
			data.Result.startingAttackers = RavageBehavior.GetAttackers( this, data );
			data.Result.startingDefenders = RavageBehavior.GetDefenders( this, data );

			await RavageSequence( this, data );

			data.GameState.Log( new Log.RavageEntry( data.Result ) );
		}
		finally {
			if(scope != null) {
				await scope.DisposeAsync();
				data.InvaderBinding = null;
			}
		}
	}

	static async Task RavageSequence_Default( RavageBehavior behavior, RavageData data ) {

		var ravageRounds = new RavageOrder[] { RavageOrder.Ambush, RavageOrder.InvaderTurn, RavageOrder.DahanTurn };
		foreach(RavageOrder ravageRound in ravageRounds) {
			int damageInflictedByAttackers = await GetDamageInflictedByAttackers( behavior, data, ravageRound );
			int damageFromDefenders = RavageBehavior.GetDamageInflictedByDefenders( behavior, data, ravageRound );
			await DamageAttackers( data, damageFromDefenders );
			await DamageDefenders( behavior, data, damageInflictedByAttackers );
			await DamageLand( data, damageInflictedByAttackers );
		}

	}

	static public async Task DamageDefenders( RavageBehavior behavior, RavageData data, int damageInflictedFromAttackers ) {

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
			.OfType<HumanToken>()
			.OrderByDescending( x => x.RemainingHealth )
			.ThenBy( x => x.StrifeCount ) // non strifed first
			.ToArray();
		if(participatingExplorers.Length > 0) {
			foreach(var token in participatingExplorers) {
				int tokensToDestroy = Math.Min( data.Result.startingDefenders[token], damageToApply / token.RemainingHealth );
				// destroy real tokens
				await data.InvaderBinding.DestroyNTokens( token, tokensToDestroy );
				// update our defenders count
				defenders[token] -= tokensToDestroy;
				damageToApply -= tokensToDestroy * token.RemainingHealth;
			}
		}

		// !! if we had cities / towns helping with defend, we would want to apply partial damage to them here.

		if( data.Tokens.ModsOfType<IStopDahanDamage>().Any() ) return;

		// Dahan
		var damagableDahan = defenders.Keys
			.Cast<HumanToken>()
			.Where( k => k.Class == Human.Dahan ) // Normal only, filters out frozen/sleeping
			.OrderBy( t => t.RemainingHealth ) // kill damaged dahan first
			.ToArray();

		var dahan = new DahanBinding( data.Tokens );

		foreach(var dahanToken in damagableDahan) {

			// 1st - Destroy weekest dahan first
			int tokensToDestroy = Math.Min(
				data.Result.startingDefenders[dahanToken],
				damageToApply / dahanToken.RemainingHealth // rounds down
			);
			if(0 < tokensToDestroy) {

				int removed = await dahan.Destroy( tokensToDestroy, dahanToken );

				// use up damage
				damageToApply -= tokensToDestroy * dahanToken.RemainingHealth;

				data.Result.dahanDestroyed += removed;
				defenders[dahanToken] -= removed;
			}
			// damage real tokens

			// 2nd - if we no longer have enougth to destroy this token, apply damage all the damage that remains
			if(0 < defenders[dahanToken] && 0 < damageToApply) {

				int remainingDamage = await dahan.ApplyDamage_Efficiently( damageToApply, dahanToken ); // this will never destroy token

				// update our defenders count
				if(remainingDamage < damageInflictedFromAttackers) { // if we actually did damage
					// !!! not sure this is 100% correct - if dahan can't be damaged, shouldn't do this update below
					++defenders[dahanToken.AddDamage( damageToApply )];
					--defenders[dahanToken];
				}
				damageToApply = 0;
			}

		}

	}

	/// <summary> Defend has already been applied. </summary>
	static public async Task<int> GetDamageInflictedByAttackers( RavageBehavior behavior, RavageData data, RavageOrder round ) {

		if(round != RavageOrder.InvaderTurn) return 0; // Currently nothing changes invaders RavageRound so this is simplest

		// CurrentAttackers
		int rawDamageFromAttackers = behavior.GetDamageFromParticipatingAttackers( behavior, data.Result.startingAttackers, data.Tokens );

		data.CurrentAttackers = await FromEachStrifed_RemoveOneStrife( data ); // does not change start state, modifies gs.Tokens[...] instead

		// Defend
		data.Result.defend = data.Tokens.Defend.Count;
		int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - data.Result.defend, 0 );

		return damageInflictedFromAttackers;
	}

	/// <returns>New attacker finvaders</returns>
	static async Task<CountDictionary<HumanToken>> FromEachStrifed_RemoveOneStrife( RavageData ra ) {
		CountDictionary<HumanToken> participatingInvaders = ra.Result.startingAttackers;

		var newAttackers = participatingInvaders.Clone();

		var strifed = participatingInvaders.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount > 0 )
			.OrderBy( x => x.StrifeCount ) // smallest first
			.ToArray();
		foreach(var orig in strifed) {
			// update tracking counts
			int count = ra.Tokens[orig];
			newAttackers[orig] -= count;
			newAttackers[orig.AddStrife( -1 )] += count;

			// update real tokens
			await ra.Tokens.Remove1StrifeFrom( orig, ra.Tokens[orig] );
		}

		return newAttackers;
	}

	static public int GetDamageInflictedByDefenders( RavageBehavior behavior, RavageData data, RavageOrder round ) {
		CountDictionary<HumanToken> participants = GetDefenders(behavior,data);
		foreach(var participant in participants.Keys.ToArray())
			if(participant.RavageOrder != round)
				participants[participant] = 0;

		int damageFromDefenders = participants.Sum( pair => pair.Key.Attack * pair.Value );
		return Math.Max( 0, damageFromDefenders - behavior.AttackersDefend );
	}

	static int GetDamageFromParticipatingAttackers_Default( RavageBehavior behavior, CountDictionary<HumanToken> participatingAttacker, SpaceState tokens ) {
		return participatingAttacker.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount == 0 )
			.Select( attacker => attacker.Attack * participatingAttacker[attacker] ).Sum();
	}

	/// <returns>(city-dead,town-dead,explorer-dead)</returns>
	static public async Task DamageAttackers( RavageData ra, int damageFromDefenders ) {
		ra.Result.attackerDamageFromDefenders = damageFromDefenders;
		if(damageFromDefenders == 0) return;

		ra.Result.attackerDamageFromBadlands = ra.BadLandsCount;
		int remainingDamageToApply = damageFromDefenders + ra.BadLandsCount;

		// ! must use current Attackers counts, because some have lost their strife so tokens are different than when they started.
		var remaningAttackers = ra.CurrentAttackers.Clone();

		while(0 < remainingDamageToApply && remaningAttackers.Any()) {
			HumanToken attackerToDamage = PickSmartInvaderToDamage( remaningAttackers, remainingDamageToApply );

			// Calc Damage to apply to 1 invader
			int damageToApplyToAttacker = Math.Min( remainingDamageToApply, attackerToDamage.RemainingHealth );
			remainingDamageToApply -= damageToApplyToAttacker; // damage we apply and damage inflicted may be different

			var (actualDamageInflicted, _) = await ra.InvaderBinding.ApplyDamageTo1( damageToApplyToAttacker, attackerToDamage );

			// Apply tracking damage
			--remaningAttackers[attackerToDamage];
			var damagedAttacker = attackerToDamage.AddDamage( actualDamageInflicted );
			if(damagedAttacker.RemainingHealth > 0) ++remaningAttackers[damagedAttacker];

		}
		ra.Result.endingAttackers = remaningAttackers;
	}

	static public async Task DamageLand( RavageData ra, int damageInflictedFromInvaders ) {
		if(damageInflictedFromInvaders == 0) return;

		await ra.InvaderBinding.Tokens
			.Add(LandDamage.Token, damageInflictedFromInvaders );
	}

	#region Static Smart Damage to Invaders

	public static HumanToken PickSmartInvaderToDamage( CountDictionary<HumanToken> participatingInvaders, int availableDamage ) {
		return PickItemToKill( participatingInvaders.Keys, availableDamage )
			?? PickItemToDamage( participatingInvaders.Keys );
	}

	public static HumanToken PickItemToKill( IEnumerable<HumanToken> candidates, int availableDamage ) {
		return candidates
			.Where( specific => specific.RemainingHealth <= availableDamage ) // can be killed
			.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
			.ThenBy( k => k.RemainingHealth ) // and most damaged.
			.FirstOrDefault();
	}

	public static HumanToken PickItemToDamage( IEnumerable<HumanToken> candidates ) {
		return candidates
			.OrderBy( i => i.RemainingHealth ) // closest to dead
			.ThenByDescending( i => i.FullHealth ) // biggest impact
			.First();
	}

	#endregion

	public static CountDictionary<HumanToken> GetAttackers( RavageBehavior behavior, RavageData data ) 
		=> GetParticipantCounts( behavior, data, RavageSide.Attacker );

	static public CountDictionary<HumanToken> GetDefenders( RavageBehavior behavior, RavageData data ) 
		=> GetParticipantCounts( behavior, data, RavageSide.Defender );

	static CountDictionary<HumanToken> GetParticipantCounts( RavageBehavior cfg, RavageData ra, RavageSide side ) {
		var participants = new CountDictionary<HumanToken>();
		foreach(var token in ra.Tokens.OfTypeHuman().Where( token => token.RavageSide == side ))
			participants[token] = Math.Max( 0, ra.Tokens[token] - cfg.NotParticipating[token] );
		return participants;
	}

}