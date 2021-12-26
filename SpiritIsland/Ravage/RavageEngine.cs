using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class RavageEngine {

		readonly protected InvaderBinding grp;
		readonly Func<Space,int,Task> damageLandCallback;
		readonly protected ConfigureRavage cfg;

		#region constructor

		public RavageEngine( GameState gs, InvaderBinding grp, ConfigureRavage cfg ) {
			this.grp = grp;
			this.cfg = cfg;
			damageLandCallback = gs.DamageLandFromRavage;
		}

		#endregion

		InvadersRavaged @event;

		public async Task<InvadersRavaged> Exec() {
			if(!Tokens.HasInvaders()) return null;
			RecordStartingState();
			await RavageSequence();
			return @event;
		}

		void RecordStartingState() {
			@event = new InvadersRavaged {
				Space = grp.Space,
				startingInvaders = grp.Tokens.InvaderSummary,
				startingDahan = grp.Tokens.Dahan.Count,
			};
		}

		async Task RavageSequence() {
			if(cfg.RavageSequence != null) { await cfg.RavageSequence(this); return; }

			// Default Ravage Sequence
			int damageInflictedByAttackers = GetDamageInflictedByAttackers();
			await DamageDefenders( damageInflictedByAttackers );
			int damageFromDefenders = GetDamageInflictedByDefenders();
			await DamageAttackers( damageFromDefenders );

			// worry about this last - why?, just because
			await DamageLand( damageInflictedByAttackers );
		}

		#region Selecting the Attacker / Defender groups

		CountDictionary<Token> AttackerCounts() => GetParticipantCounts( cfg.IsAttacker ?? IsInvader );

		CountDictionary<Token> DefenderCounts() => GetParticipantCounts( cfg.IsDefender ?? IsDahan );
		static bool IsInvader(Token token) => token.Class.Category == TokenCategory.Invader;
		static bool IsDahan(Token token) => token.Class == TokenType.Dahan;

		CountDictionary<Token> GetParticipantCounts( Func<Token,bool> filter ) {
			var participants = new CountDictionary<Token>();
			foreach(var token in Tokens.Keys.Where(filter))
				participants[token] = Math.Max( 0, Tokens[token] - cfg.NotParticipating[token] );
			return participants;
		}

		#endregion

		public int GetDamageInflictedByAttackers() {

			CountDictionary<Token> participatingAttackers = AttackerCounts();

			int rawDamageFromAttackers = RawDamageFromParticipatingAttackers( participatingAttackers );

			FromEachStrifed_RemoveOneStrife( participatingAttackers );

			// Defend
			int damageInflictedFromAttackers = Math.Max( rawDamageFromAttackers - Tokens.Defend.Count, 0 );

			return damageInflictedFromAttackers;
		}

		public int GetDamageInflictedByDefenders() {
			CountDictionary<Token> participants = DefenderCounts();
			return participants.Sum( pair => pair.Key.FullHealth * pair.Value );
		}

		public async Task DamageLand( int damageInflictedFromInvaders ) {
			if( cfg.ShouldDamageLand )
				await damageLandCallback(grp.Space,damageInflictedFromInvaders);
		}

		/// <returns># of dahan killed/destroyed</returns>
		public async Task DamageDefenders( int damageFromAttackers ) {
			@event.damageFromAttackers = damageFromAttackers;
			if(damageFromAttackers == 0 || !cfg.ShouldDamageDahan) return;

			int defendDamageTotal = damageFromAttackers
				+ BadLandsCount;

			// destroy dahan
			var dahan = grp.Tokens.Dahan;
			@event.startingDahan = dahan.Count;
			@event.dahanDestroyed = Math.Min( defendDamageTotal / cfg.DahanHitpoints, @event.startingDahan ); // rounding down, !!! if some of the dahan have previously taken damage, this # will be wrong
			if(@event.dahanDestroyed != 0)
				await cfg.DestroyDahan( dahan, @event.dahanDestroyed, 2 );

			int leftOverDamage = defendDamageTotal - @event.dahanDestroyed * cfg.DahanHitpoints;
			bool convert1To1HitPoint = leftOverDamage == cfg.DahanHitpoints - 1;
			if(convert1To1HitPoint && dahan.Any)
				await dahan.ApplyDamage(1,Cause.Invaders);

		}

		/// <returns>(city-dead,town-dead,explorer-dead)</returns>
		public async Task DamageAttackers( int damageFromDefenders ) {
			@event.damageFromDefenders = damageFromDefenders;
			if(damageFromDefenders == 0) return;

			int remainingDamageToApply = damageFromDefenders + BadLandsCount;

			var attackers = AttackerCounts();
			while(remainingDamageToApply > 0 && attackers.Any()) {
				Token invadertodamage = PickSmartInvaderToDamage( attackers, remainingDamageToApply );
				var (damageInflicted,_) = await grp.ApplyDamageTo1( remainingDamageToApply, invadertodamage, true );
				remainingDamageToApply -= damageInflicted;
				// update participants
				attackers = AttackerCounts();
			}
			@event.endingInvaders = grp.Tokens.InvaderSummary;
		}

		#region private

		int RawDamageFromParticipatingAttackers( CountDictionary<Token> participatingAttacker ) {

			return participatingAttacker.Keys
				.Where( x => x is not StrifedInvader )
				.Select( attacker => cfg.DamageFromAttacker( attacker ) * participatingAttacker[attacker] ).Sum();
		}


		void FromEachStrifed_RemoveOneStrife( CountDictionary<Token> participatingInvaders ) {
			var strifed = participatingInvaders.Keys.OfType<StrifedInvader>()
				.OrderBy( x => x.StrifeCount ) // smallest first
				.ToArray();
			foreach(var orig in strifed)
				Tokens.RemoveStrife( orig, Tokens[orig] );
		}

		int BadLandsCount => Tokens.Badlands.Count;

		TokenCountDictionary Tokens => grp.Tokens;

		#endregion

		#region Static Smart Damage to Invaders

		static Token PickSmartInvaderToDamage( CountDictionary<Token> participatingInvaders, int availableDamage) {
			return PickItemToKill( participatingInvaders.Keys, availableDamage )
				?? PickItemToDamage( participatingInvaders.Keys );
		}

		static Token PickItemToKill(IEnumerable<Token> candidates, int availableDamage) {
			return candidates
				.Where( specific => specific.Health <= availableDamage ) // can be killed
				.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
				.ThenBy( k => k.Health ) // and most damaged.
				.FirstOrDefault();
		}

		static Token PickItemToDamage( IEnumerable<Token> candidates ) {
			return candidates
				.OrderBy(i=>i.Health) // closest to dead
				.ThenByDescending(i=>i.FullHealth) // biggest impact
				.First();
		}

		#endregion

	}


}
