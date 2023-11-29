namespace SpiritIsland;

/// <summary>
/// Configures Dahan and Invader behavior on a per-space bases.
/// </summary>
public sealed class RavageBehavior : ISpaceEntity, IEndWhenTimePasses {

	public static RavageBehavior DefaultBehavior => GameState.Current.DefaultRavageBehavior;

	// Order / Who is damaged
	public Func<RavageBehavior, RavageData, Task> RavageSequence = RavageSequence_Default;

	// Gets the Aggregate damage from attackers. (default action does this by calling AttackDamageFrom1)
	// !! This can be removed if we slap a Town-Tracking token on every space Which updates a 'NeighboringTownsToken' that does damage.
	public Func<RavageExchange,int> GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers_Default;

	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	public RavageBehavior Clone() {
		return new RavageBehavior {
			RavageSequence = RavageSequence,
			GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers,
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

		// Config the ravage
		foreach(IConfigRavagesAsync configurer in data.Tokens.ModsOfType<IConfigRavagesAsync>().ToArray() )
			await configurer.ConfigAsync( data.Tokens );
		foreach(IConfigRavages configurer in data.Tokens.ModsOfType<IConfigRavages>().ToArray() )
			configurer.Config( data.Tokens );

		data.InvaderBinding = data.Tokens.Invaders;

		try {
			await RavageSequence( this, data );

			GameState.Current.Log( new Log.RavageEntry( data.Result.ToArray() ) );
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
			var exchange = new RavageExchange(data.Tokens,ravageRound);
			if(!exchange.HasActiveParticipants) continue;

			GetDamageInflictedByAttackers( exchange, behavior );				// So Hapsburg monarchy can add +2 damage from neighboring towns
			GetDamageInflictedByDefenders( exchange, behavior.AttackersDefend );
			await DamageAttackers( exchange );
			await DamageDefenders( exchange );
			data.Result.Add( exchange );
		}
		await DamageLand( data );
	}

	static public async Task DamageDefenders( RavageExchange rex ) {

		// Start with Damage from attackers
		if(rex.damageFromAttackers == 0) return;

		// Add Badlands damage
		rex.defenderDamageFromBadlands = rex.Tokens.Badlands.Count;
		int damageToApply = rex.damageFromAttackers + rex.defenderDamageFromBadlands; // to defenders

		var defenders = rex.StartingDefenders.Clone();

		// Damage helping Invaders
		// When damaging defenders, it is ok to damage the explorers first.
		// https://querki.net/u/darker/spirit-island-faq/#!Voice+of+Command 
		var participatingExplorers = defenders.Keys
			.Where( k => k.HumanClass.HasAny(Human.Invader) )
			.OfType<HumanToken>()
			.OrderByDescending( x => x.RemainingHealth ) // kill lowest health first (must be efficient)
			.ThenBy( x => x.StrifeCount ) // non strifed first
			.ToArray();
		if(participatingExplorers.Length > 0) {
			foreach(var token in participatingExplorers) {
				int tokensToDestroy = Math.Min( rex.StartingDefenders[token], damageToApply / token.RemainingHealth );
				// destroy real tokens
				await rex.Tokens.Invaders.DestroyNTokens( token, tokensToDestroy );
				// update our defenders count
				defenders[token] -= tokensToDestroy;
				damageToApply -= tokensToDestroy * token.RemainingHealth;
			}
		}

		// !! if we had cities / towns helping with defend, we would want to apply partial damage to them here.

//		if( rex.Tokens.ModsOfType<IStopDahanDamage>().Any() ) return;

		// Dahan
		var damagableDahan = defenders.Keys
			.Cast<HumanToken>()
			.Where( k => k.HumanClass == Human.Dahan ) // Normal only, filters out frozen/sleeping
			.OrderBy( t => t.RemainingHealth ) // kill damaged dahan first
			.ThenBy( x => x.RavageOrder ) // but if 2 have same health, pick the one that has already attacked
			.ToArray();

		var dahan = rex.Tokens.Dahan;

		foreach(var dahanToken in damagableDahan) {

			// 1st - Destroy weekest dahan first
			int tokensToDestroy = Math.Min(
				rex.StartingDefenders[dahanToken],
				damageToApply / dahanToken.RemainingHealth // rounds down
			);
			if(0 < tokensToDestroy) {

				int removed = await dahan.Destroy( tokensToDestroy, dahanToken );

				// use up damage
				damageToApply -= tokensToDestroy * dahanToken.RemainingHealth;

				rex.dahanDestroyed += removed;
				defenders[dahanToken] -= removed;
			}
			// damage real tokens

			// 2nd - if we no longer have enougth to destroy this token, apply damage all the damage that remains
			if(0 < defenders[dahanToken] && 0 < damageToApply) {

				int remainingDamage = await dahan.ApplyDamageToAll_Efficiently( damageToApply, dahanToken ); // this will never destroy token

				// update our defenders count
				if(remainingDamage < rex.damageFromAttackers) { // if we actually did damage
					// !!! not sure this is 100% correct - if dahan can't be damaged, shouldn't do this update below
					++defenders[dahanToken.AddDamage( damageToApply )];
					--defenders[dahanToken];
				}
				damageToApply = 0;
			}

		}

		rex.EndingDefenders = defenders;

	}


	/// <summary> Defend has already been applied. </summary>
	static public void GetDamageInflictedByAttackers( RavageExchange rex, RavageBehavior behavior ) {

		// CurrentAttackers
		int rawDamageFromAttackers = behavior.GetDamageFromParticipatingAttackers( rex );

		// Defend
		rex.defend = rex.Tokens.Defend.Count;
		int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - rex.defend, 0 );

		rex.damageFromAttackers = damageInflictedFromAttackers; // Defend points already applied.
	}

	/// <returns>New attacker finvaders</returns>
	static async Task<CountDictionary<HumanToken>> FromEachStrifed_RemoveOneStrife( RavageExchange rex ) {

		CountDictionary<HumanToken> participatingInvaders = rex.StartingAttackers;

		var newAttackers = participatingInvaders.Clone();

		var strifed = participatingInvaders.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount > 0 )
			.OrderBy( x => x.StrifeCount ) // smallest first
			.ToArray();
		foreach(var orig in strifed) {
			// update tracking counts
			int count = rex.Tokens[orig];
			newAttackers[orig] -= count;
			newAttackers[orig.AddStrife( -1 )] += count;

			// update real tokens
			await rex.Tokens.Remove1StrifeFrom( orig, rex.Tokens[orig] );
		}

		return newAttackers;
	}

	static public void GetDamageInflictedByDefenders( RavageExchange rex, int attackersDefend ) {
		
		int damageFromDefenders = rex.ActiveDefenders
			.Sum( pair => pair.Key.Attack * pair.Value );

		rex.damageFromDefenders = Math.Max( 0, damageFromDefenders - attackersDefend );
	}

	static int GetDamageFromParticipatingAttackers_Default( RavageExchange rex ) {
		SpaceState tokens = rex.Tokens;
		return rex.ActiveAttackers.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount == 0 )
			.Select( attacker => attacker.Attack * rex.StartingAttackers[attacker] ).Sum();
	}

	/// <returns>(city-dead,town-dead,explorer-dead)</returns>
	static public async Task DamageAttackers( RavageExchange rex ) {
		if(rex.damageFromDefenders == 0){
			rex.EndingAttackers = rex.StartingAttackers;
			return;
		}

		int badlands = rex.Tokens.Badlands.Count;
		rex.attackerDamageFromBadlands = badlands;
		int remainingDamageToApply = rex.damageFromDefenders + badlands;

		// ! must use current Attackers counts, because some have lost their strife so tokens are different than when they started.

		var remaningAttackers = await FromEachStrifed_RemoveOneStrife( rex ); // does not change start state, modifies gs.Tokens[...] instead

		while(0 < remainingDamageToApply && remaningAttackers.Any()) {
			HumanToken attackerToDamage = PickSmartInvaderToDamage( remaningAttackers, remainingDamageToApply, rex.Order );

			// Calc Damage to apply to 1 invader
			int damageToApplyToAttacker = Math.Min( remainingDamageToApply, attackerToDamage.RemainingHealth );
			remainingDamageToApply -= damageToApplyToAttacker; // damage we apply and damage inflicted may be different

			var (actualDamageInflicted, _) = await rex.Tokens.Invaders.ApplyDamageTo1( damageToApplyToAttacker, attackerToDamage );

			// Apply tracking damage
			--remaningAttackers[attackerToDamage];
			var damagedAttacker = attackerToDamage.AddDamage( actualDamageInflicted );
			if(damagedAttacker.RemainingHealth > 0) ++remaningAttackers[damagedAttacker];

		}
		rex.EndingAttackers = remaningAttackers;
	}

	static public async Task DamageLand( RavageData ra ) {
		int totalLandDamage = ra.Result.Sum( r => r.damageFromAttackers );
		if(totalLandDamage == 0) return;

		await LandDamage.Add( ra.InvaderBinding.Tokens, totalLandDamage );
	}

	#region Static Smart Damage to Invaders

	public static HumanToken PickSmartInvaderToDamage( CountDictionary<HumanToken> participatingInvaders, int availableDamage, RavageOrder order ) {

		return Pick_HighestHealthThatIsKillable( participatingInvaders.Keys, availableDamage, order == RavageOrder.Ambush )
			?? Pick_ItemClosestToDead( participatingInvaders.Keys );
	}

	static HumanToken Pick_HighestHealthThatIsKillable( IEnumerable<HumanToken> candidates, int availableDamage, bool pickUnstrifedFirst ) {
		return candidates
			.Where( specific => specific.RemainingHealth <= availableDamage ) // can be killed
			.OrderBy( specific => pickUnstrifedFirst && specific.StrifeCount == 0 ? 0 : 1 )
			.ThenByDescending( k => k.FullHealth ) // pick items with most Full Health
			.ThenBy( k => k.RemainingHealth ) // and most damaged.
			.FirstOrDefault();
	}

	static HumanToken Pick_ItemClosestToDead( IEnumerable<HumanToken> candidates ) {
		return candidates
			.OrderBy( i => i.RemainingHealth ) // closest to dead
			.ThenByDescending( i => i.FullHealth ) // biggest impact
			.First();
	}

	#endregion

}