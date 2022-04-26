namespace SpiritIsland;

public class RavageAction {

	readonly protected InvaderBinding grp;
	readonly Func<Space,int,Task> damageLandCallback;
	readonly protected ConfigureRavage cfg;

	readonly GameState gameState;

	#region constructor

	public RavageAction( GameState gs, InvaderBinding grp ) {
		var cfg = gs.GetRavageConfiguration( grp.Space );

		this.gameState = gs;
		this.grp = grp;
		this.cfg = cfg;
		damageLandCallback = gs.DamageLandFromRavage;

		@event = new InvadersRavaged { Space = grp.Space };
	}

	#endregion

	readonly InvadersRavaged @event; // record status here
	CountDictionary<HealthToken> currentAttackers; // tokens might change if strife is removed

	public async Task<InvadersRavaged> Exec() {
		if(!Tokens.HasInvaders()) return null;

		// Record starting state
		@event.startingAttackers = GetAttackers();
		@event.startingDefenders = GetDefenders();

		await RavageSequence();
		await gameState.InvadersRavaged.InvokeAsync(@event);
		return @event;
	}

	async Task RavageSequence() {
		if(cfg.RavageSequence != null) { 
			await cfg.RavageSequence(this); 
			return;
		}

		// Default Ravage Sequence
		int damageInflictedByAttackers = GetDamageInflictedByAttackers();
		await DamageDefenders( damageInflictedByAttackers );
		int damageFromDefenders = GetDamageInflictedByDefenders();
		await DamageAttackers( damageFromDefenders );

		// worry about this last - why?, just because
		await DamageLand( damageInflictedByAttackers );
	}

	#region Selecting the Attacker / Defender groups

	CountDictionary<HealthToken> GetAttackers() => GetParticipantCounts( cfg.IsAttacker ?? IsInvader );

	CountDictionary<HealthToken> GetDefenders() => GetParticipantCounts( cfg.IsDefender ?? IsDahan );
	static bool IsInvader(Token token) => token.Class.Category == TokenCategory.Invader;
	static bool IsDahan(Token token) => token.Class == TokenType.Dahan;

	CountDictionary<HealthToken> GetParticipantCounts( Func<HealthToken,bool> filter ) {
		var participants = new CountDictionary<HealthToken>();
		foreach(var token in Tokens.Keys.OfType<HealthToken>().Where(filter))
			participants[token] = Math.Max( 0, Tokens[token] - cfg.NotParticipating[token] );
		return participants;
	}

	#endregion

	/// <summary>
	/// Defend has already been applied.
	/// </summary>
	public int GetDamageInflictedByAttackers() {

		int rawDamageFromAttackers = RawDamageFromParticipatingAttackers( @event.startingAttackers );

		this.currentAttackers = FromEachStrifed_RemoveOneStrife( @event.startingAttackers ); // does not change start state, modifies gs.Tokens[...] instead

		// Defend
		@event.defend = Tokens.Defend.Count;
		int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - @event.defend, 0 );

		return damageInflictedFromAttackers;
	}

	public int GetDamageInflictedByDefenders() {
		CountDictionary<HealthToken> participants = GetDefenders();
		return participants.Sum( pair => pair.Key.Class.Attack * pair.Value );
	}

	public async Task DamageLand( int damageInflictedFromInvaders ) {
		if( cfg.ShouldDamageLand )
			await damageLandCallback(grp.Space,damageInflictedFromInvaders);
	}

	/// <returns># of dahan killed/destroyed</returns>
	public async Task DamageDefenders( int damageInflictedFromAttackers ) { 
		// Defend point have already been applied!

		// Start with Damage from attackers
		@event.defenderDamageFromAttackers = damageInflictedFromAttackers;
		if(damageInflictedFromAttackers == 0 || !cfg.ShouldDamageDahan) return;

		// Add Badlands damage
		@event.defenderDamageFromBadlands = BadLandsCount;
		int damageToApply = damageInflictedFromAttackers + BadLandsCount; // to defenders

		var defenders = @event.startingDefenders.Clone();

		// Damage helping Invaders
		// When damaging defenders, it is ok to damage the explorers first.
		// https://querki.net/u/darker/spirit-island-faq/#!Voice+of+Command 
		var participatingExplorers = defenders.Keys
			.OfType<HealthToken>()
			.Where(k=>k.Class.Category == TokenCategory.Invader)
			.OrderByDescending(x=>x.RemainingHealth)
			.ThenBy(x=>x.StrifeCount) // non strifed first
			.ToArray();
		if( participatingExplorers.Length > 0) {
			foreach(var token in participatingExplorers) {
				int tokensToDestroy = Math.Min(@event.startingDefenders[token], damageToApply / token.RemainingHealth );
				// destroy real tokens
				await grp.Destroy(tokensToDestroy,token);
				// update our defenders count
				defenders[token] -= tokensToDestroy;
				damageToApply -= tokensToDestroy * token.RemainingHealth;
			}
		}

		// !! if we had cities / towns helping with defend, we would want to apply partial damage to them here.

		// Dahan
		if(cfg.ShouldDamageDahan) {
			var participatingDahan = defenders.Keys
				.Cast<HealthToken>()
				.Where(k=>k.Class.Category == TokenCategory.Dahan)
				.OrderBy(t=>t.RemainingHealth) // kill damaged dahan first
				.ToArray();

			var dahan = new DahanGroupBinding( Tokens, RemoveReason.DestroyedInBattle ) { Frozen = Tokens.Dahan.Frozen };

			foreach(var token in participatingDahan) {

				// 1st - Destroy weekest dahan first
				int tokensToDestroy = Math.Min(@event.startingDefenders[token], damageToApply/token.RemainingHealth); // rounds down
				if(tokensToDestroy > 0) {

					await cfg.DestroyDahan( dahan, tokensToDestroy, token );

					@event.dahanDestroyed += tokensToDestroy;
					defenders[token] -= tokensToDestroy;
					damageToApply -= tokensToDestroy * token.RemainingHealth;
				}
				// damage real tokens

				// 2nd - if we no longer have enougth to destroy this token, apply damage all the damage that remains
				if(0 < defenders[token] && 0 < damageToApply) {

					await dahan.ApplyDamageToToken( damageToApply, token );

					// update our defenders count
					++defenders[token.AddDamage( damageToApply )];
					--defenders[token];
					damageToApply = 0;
				}

			}

		}

	}

	/// <returns>(city-dead,town-dead,explorer-dead)</returns>
	public async Task DamageAttackers( int damageFromDefenders ) {
		@event.attackerDamageFromDefenders = damageFromDefenders;
		if(damageFromDefenders == 0) return;

		@event.attackerDamageFromBadlands = BadLandsCount;
		int remainingDamageToApply = damageFromDefenders + BadLandsCount;

		// ! must use current Attackers counts, because some have lost their strife so tokens are different than when they started.
		var remaningAttackers = currentAttackers.Clone();

		while(remainingDamageToApply > 0 && remaningAttackers.Any()) {
			HealthToken attackerToDamage = PickSmartInvaderToDamage( remaningAttackers, remainingDamageToApply );

			// Apply real damage
			var (damageInflicted,_) = await grp.ApplyDamageTo1( remainingDamageToApply, attackerToDamage, true );
			remainingDamageToApply -= damageInflicted;

			// Apply tracking damage
			--remaningAttackers[attackerToDamage];
			var damagedAttacker = attackerToDamage.AddDamage( damageInflicted );
			if(damagedAttacker.RemainingHealth>0) ++remaningAttackers[damagedAttacker];

		}
		@event.endingAttackers = remaningAttackers;
	}

	#region private

	int RawDamageFromParticipatingAttackers( CountDictionary<HealthToken> participatingAttacker ) {

		return participatingAttacker.Keys
			.OfType<HealthToken>()
			.Where( x => x.StrifeCount==0 )
			.Select( attacker => cfg.DamageFromAttacker( attacker ) * participatingAttacker[attacker] ).Sum();
	}


	/// <returns>New attacker finvaders</returns>
	CountDictionary<HealthToken> FromEachStrifed_RemoveOneStrife( CountDictionary<HealthToken> participatingInvaders ) {

		var newAttackers = participatingInvaders.Clone();

		var strifed = participatingInvaders.Keys
			.OfType<HealthToken>()
			.Where(x=>x.StrifeCount>0)
			.OrderBy( x => x.StrifeCount ) // smallest first
			.ToArray();
		foreach(var orig in strifed) {
			// update tracking counts
			int count = Tokens[orig];
			newAttackers[orig] -= count;
			newAttackers[orig.AddStrife( -1 )] += count;

			// update real tokens
			Tokens.RemoveStrife( orig, Tokens[orig] );
		}

		return newAttackers;
	}

	int BadLandsCount => Tokens.Badlands.Count;

	TokenCountDictionary Tokens => grp.Tokens;

	#endregion

	#region Static Smart Damage to Invaders

	static HealthToken PickSmartInvaderToDamage( CountDictionary<HealthToken> participatingInvaders, int availableDamage) {
		return PickItemToKill( participatingInvaders.Keys, availableDamage )
			?? PickItemToDamage( participatingInvaders.Keys );
	}

	static HealthToken PickItemToKill(IEnumerable<HealthToken> candidates, int availableDamage) {
		return candidates
			.Where( specific => specific.RemainingHealth <= availableDamage ) // can be killed
			.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
			.ThenBy( k => k.RemainingHealth ) // and most damaged.
			.FirstOrDefault();
	}

	static HealthToken PickItemToDamage( IEnumerable<HealthToken> candidates ) {
		return candidates
			.OrderBy(i=>i.RemainingHealth) // closest to dead
			.ThenByDescending(i=>i.FullHealth) // biggest impact
			.First();
	}

	#endregion

}