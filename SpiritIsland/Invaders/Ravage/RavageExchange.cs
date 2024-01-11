namespace SpiritIsland;

public class RavageParticipants {

	public RavageParticipants(
		CountDictionary<HumanToken> starting,
		CountDictionary<HumanToken> active
	) {
		Starting = starting;
		Active = active;
	}

	/// <summary> Anyone that can receive damage. </summary>
	readonly public CountDictionary<HumanToken> Starting;

	/// <summary> The participants that deal damage. </summary>
	readonly public CountDictionary<HumanToken> Active;

	/// <summary> Damage dealt out to the other side. (less any Defend) </summary>
	public int DamageDealtOut;

	public int DamageReceivedFromBadlands;

	public CountDictionary<HumanToken> Ending;

}

public class RavageExchange {

	/// <summary> Creates a standard Ravage Exchange. </summary>
	public RavageExchange( SpaceState tokens, RavageOrder order ):this(
		tokens,
		order, 
		GetSideParticipants( tokens, RavageSide.Attacker, order ),
		GetSideParticipants( tokens, RavageSide.Defender, order ) 
	) { }

	/// <summary> Creates a Custom Ravage Exchange. </summary>
	public RavageExchange( SpaceState tokens, RavageOrder order, RavageParticipants attackers, RavageParticipants defenders ) {
		Tokens = tokens;
		Order = order;
		Attackers = attackers;
		Defenders = defenders;
	}

	readonly public SpaceState Tokens;
	readonly public RavageOrder Order;
	readonly public RavageParticipants Attackers;
	readonly public RavageParticipants Defenders;

	public Space Space => Tokens.Space;
	public bool HasActiveParticipants => 0 < (Attackers.Active.Count + Defenders.Active.Count);

	public int Defend;
	public int DahanDestroyed;

	static RavageParticipants GetSideParticipants( SpaceState tokens, RavageSide side, RavageOrder order ) {

		var starting = tokens.AllHumanTokens()
			.Where( token => token.RavageSide == side )
			.ToDictionary( x => x, x => tokens[x] )
			.ToCountDict();

		var active = starting.Keys
			.Where( token => token.RavageOrder == order )
			.ToDictionary( x => x, x => tokens[x] )
			.ToCountDict();

		return new RavageParticipants( starting, active );
	}

	public async Task Execute( RavageBehavior behavior ) {
		GetDamageInflictedByAttackers( behavior );             // So Hapsburg monarchy can add +2 damage from neighboring towns
		GetDamageInflictedByDefenders( behavior.AttackersDefend );
		await DamageAttackers();
		await DamageDefenders();
	}

	public override string ToString() {

		string verb = Order switch {
			RavageOrder.Ambush => "ambush",
			RavageOrder.InvaderTurn => "attack",
			RavageOrder.DahanTurn => "defend",
			_ => "Unknown"
		};

		var parts = new List<string> {
			$"{verb}:"
		};

		if(0 < Attackers.Active.Count) {
			if(Defend > 0)
				parts.Add( $"Defend {Defend}." );

			// Attacker Effect
			string badlandString = 0 < Defenders.DamageReceivedFromBadlands ? $" plus {Defenders.DamageReceivedFromBadlands} badland damage" : string.Empty;
			parts.Add( $"({Attackers.Active.TokenSummary()}) deal {Attackers.DamageDealtOut} damage{badlandString} to defenders ({Defenders.Starting.TokenSummary()}) destroying {DahanDestroyed} of them." );
		}

		// Defend Effect
		if(0 < Defenders.Active.Count) { //	if(0 < damageFromDefenders) {
			string bld = (0 < Attackers.DamageReceivedFromBadlands) ? $" plus {Attackers.DamageReceivedFromBadlands} badland damage" : string.Empty;
			parts.Add( $"({Defenders.Active.TokenSummary()}) deal {Defenders.DamageDealtOut} damage{bld}, leaving {Attackers.Ending.TokenSummary()}." );
		}

		return string.Join(" ",parts);
	}


	/// <summary> Defend has already been applied. </summary>
	public void GetDamageInflictedByAttackers( RavageBehavior behavior ) {

		// CurrentAttackers
		int rawDamageFromAttackers = behavior.GetDamageFromParticipatingAttackers( this );

		// Defend
		Defend = Tokens.Defend.Count;
		int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - Defend, 0 );

		Attackers.DamageDealtOut = damageInflictedFromAttackers; // Defend points already applied.
	}

	public void GetDamageInflictedByDefenders( int attackersDefend ) {
		int damageFromDefenders = Defenders.Active
			.Sum( pair => pair.Key.Attack * pair.Value );

		Defenders.DamageDealtOut = Math.Max( 0, damageFromDefenders - attackersDefend );
	}

	/// <returns>(city-dead,town-dead,explorer-dead)</returns>
	public async Task DamageAttackers() {
		
		if(Defenders.DamageDealtOut == 0) {
			Attackers.Ending = Attackers.Starting;
			return;
		}

		int badlands = Tokens.Badlands.Count;
		Attackers.DamageReceivedFromBadlands = badlands;
		int remainingDamageToApply = Defenders.DamageDealtOut + badlands;

		// ! must use current Attackers counts, because some have lost their strife so tokens are different than when they started.

		var remaningAttackers = await FromEachStrifed_RemoveOneStrife( this ); // does not change start state, modifies gs.Tokens[...] instead

		while(0 < remainingDamageToApply && remaningAttackers.Any()) {
			HumanToken attackerToDamage = PickSmartInvaderToDamage( remaningAttackers, remainingDamageToApply, Order );

			// Calc Damage to apply to 1 invader
			int damageToApplyToAttacker = Math.Min( remainingDamageToApply, attackerToDamage.RemainingHealth );
			remainingDamageToApply -= damageToApplyToAttacker; // damage we apply and damage inflicted may be different

			var (actualDamageInflicted, _) = await Tokens.Invaders.ApplyDamageTo1( damageToApplyToAttacker, attackerToDamage );

			// Apply tracking damage
			--remaningAttackers[attackerToDamage];
			var damagedAttacker = attackerToDamage.AddDamage( actualDamageInflicted );
			if(damagedAttacker.RemainingHealth > 0) ++remaningAttackers[damagedAttacker];

		}

		Attackers.Ending = remaningAttackers;
	}

	/// <returns>New attacker finvaders</returns>
	static async Task<CountDictionary<HumanToken>> FromEachStrifed_RemoveOneStrife( RavageExchange rex ) {

		CountDictionary<HumanToken> participatingInvaders = rex.Attackers.Starting;

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
			await rex.Tokens.Remove1StrifeFromAsync( orig, rex.Tokens[orig] );
		}

		return newAttackers;
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

	public async Task DamageDefenders() {
		
		// Start with Damage from attackers
		if(Attackers.DamageDealtOut == 0) return;

		// Add Badlands damage
		Defenders.DamageReceivedFromBadlands = Tokens.Badlands.Count;
		int damageToApply = Attackers.DamageDealtOut + Defenders.DamageReceivedFromBadlands; // to defenders

		var defenders = Defenders.Starting.Clone();

		// Damage helping Invaders
		// When damaging defenders, it is ok to damage the explorers first.
		// https://querki.net/u/darker/spirit-island-faq/#!Voice+of+Command 
		var participatingExplorers = defenders.Keys
			.Where( k => k.HumanClass.HasAny( Human.Invader ) )
			.OfType<HumanToken>()
			.OrderByDescending( x => x.RemainingHealth ) // kill lowest health first (must be efficient)
			.ThenBy( x => x.StrifeCount ) // non strifed first
			.ToArray();
		if(participatingExplorers.Length > 0) {
			foreach(var token in participatingExplorers) {
				int tokensToDestroy = Math.Min( Defenders.Starting[token], damageToApply / token.RemainingHealth );
				// destroy real tokens
				await Tokens.Invaders.DestroyNTokens( token, tokensToDestroy );
				// update our defenders count
				defenders[token] -= tokensToDestroy;
				damageToApply -= tokensToDestroy * token.RemainingHealth;
			}
		}

		// !! if we had cities / towns helping with defend, we would want to apply partial damage to them here.

		//		if( Tokens.ModsOfType<IStopDahanDamage>().Any() ) return;

		// Dahan
		var damagableDahan = defenders.Keys
			.Cast<HumanToken>()
			.Where( k => k.HumanClass == Human.Dahan ) // Normal only, filters out frozen/sleeping
			.OrderBy( t => t.RemainingHealth ) // kill damaged dahan first
			.ThenBy( x => x.RavageOrder ) // but if 2 have same health, pick the one that has already attacked
			.ToArray();

		var dahan = Tokens.Dahan;

		foreach(var dahanToken in damagableDahan) {

			// 1st - Destroy weekest dahan first
			int tokensToDestroy = Math.Min(
				Defenders.Starting[dahanToken],
				damageToApply / dahanToken.RemainingHealth // rounds down
			);
			if(0 < tokensToDestroy) {

				int removed = await dahan.Destroy( tokensToDestroy, dahanToken );

				// use up damage
				damageToApply -= tokensToDestroy * dahanToken.RemainingHealth;

				DahanDestroyed += removed;
				defenders[dahanToken] -= removed;
			}
			// damage real tokens

			// 2nd - if we no longer have enougth to destroy this token, apply damage all the damage that remains
			if(0 < defenders[dahanToken] && 0 < damageToApply) {

				int remainingDamage = await dahan.ApplyDamageToAll_Efficiently( damageToApply, dahanToken ); // this will never destroy token

				// update our defenders count
				if(remainingDamage < Attackers.DamageDealtOut) { // if we actually did damage
																// !!! not sure this is 100% correct - if dahan can't be damaged, shouldn't do this update below
					++defenders[dahanToken.AddDamage( damageToApply )];
					--defenders[dahanToken];
				}
				damageToApply = 0;
			}

		}

		Defenders.Ending = defenders;

	}

}