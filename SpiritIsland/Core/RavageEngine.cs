using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class RavageEngine {

		readonly protected InvaderGroup grp;
		readonly Func<Space,int,Task> damageLandCallback;
		readonly protected ConfigureRavage cfg;

		public int DahanDestroyed { get; private set; }

		#region constructor

		public RavageEngine( GameState gs, InvaderGroup grp, ConfigureRavage cfg ) {
			this.grp = grp;
			this.cfg = cfg;
			damageLandCallback = gs.DamageLand;
		}

		#endregion

		InvadersRavaged tracking;

		public async Task<InvadersRavaged> Exec() {
			if(!Tokens.HasInvaders()) return null;

			tracking = new InvadersRavaged{ 
				Space = grp.Space,
				startingInvaders = grp.Tokens.InvaderSummary,
				startingDahan = grp.Tokens.Dahan.Count,
			};

			await (cfg.RavageSequence ?? DefaultRavageSequence)(this);

			return tracking;
		}

		static async Task DefaultRavageSequence( RavageEngine eng ) {
			int damageInflictedFromInvaders = eng.GetDamageInflictedByInvaders();
			await eng.DamageDahan( damageInflictedFromInvaders );
			int damageFromDahan = eng.GetDamageInflictedByDahan();
			await eng.DamageInvaders( damageFromDahan );
			// worry about this last - why?, just because
			await eng.DamageLand( damageInflictedFromInvaders );
		}

		public int GetDamageInflictedByDahan() => grp.Tokens.Dahan.Count * 2;

		public int GetDamageInflictedByInvaders() {

			var participatingInvaders = CalcParticipatingInvaders();

			int damageFromInvaders = RawDamageFromParticipatingInvaders( participatingInvaders );

			FromEachStrifed_RemoveOneStrife( participatingInvaders );

			// Defend
			int damageInflictedFromInvaders = Math.Max( damageFromInvaders - Tokens.Defend, 0 );

			return damageInflictedFromInvaders;
		}

		public CountDictionary<Token> CalcParticipatingInvaders() { 
			var participants = new CountDictionary<Token>();
			foreach(var token in Tokens.Invaders()) {
				participants[token] = Math.Max(0,Tokens[token] - cfg.NotParticipating[token]);
			}
			return participants;
		}

		public async Task DamageLand( int damageInflictedFromInvaders ) {
			if( cfg.ShouldDamageLand )
				await damageLandCallback(grp.Space,damageInflictedFromInvaders);
		}

		/// <returns># of dahan killed/destroyed</returns>
		public async Task DamageDahan( int dahanDamageFromInvaders ) {
			tracking.dahanDamageFromInvaders = dahanDamageFromInvaders;
			if(dahanDamageFromInvaders == 0 || !cfg.ShouldDamageDahan) return;

			int dahanDamageTotal = dahanDamageFromInvaders
				+ Tokens.Badlands.Count;

			// ! This special DamageDahan, uses the config to change dahan health points.

			// !!! replace this dahan-hitpoint-hack with different tokens

			// destroy dahan
			var dahan = grp.Tokens.Dahan;
			tracking.startingDahan = dahan.Count;
			tracking.dahanDestroyed = Math.Min( dahanDamageTotal / cfg.DahanHitpoints, tracking.startingDahan ); // rounding down, !!! if some of the dahan have previously taken damage, this # will be wrong
			if(tracking.dahanDestroyed != 0)
				await cfg.DestroyDahan( dahan, tracking.dahanDestroyed, 2 );

			int leftOverDamage = dahanDamageTotal - tracking.dahanDestroyed * cfg.DahanHitpoints;
			bool convert1To1HitPoint = leftOverDamage == cfg.DahanHitpoints - 1;
			if(convert1To1HitPoint && dahan.Any)
				await dahan.ApplyDamage(1,Cause.Invaders);

		}

		/// <returns>(city-dead,town-dead,explorer-dead)</returns>
		public async Task DamageInvaders( int damageFromDahan ) {
			tracking.damageFromDahan = damageFromDahan;
			if(damageFromDahan == 0) return;

			int remainingDamageToApply = damageFromDahan
				+ Tokens.Badlands.Count;

			var participatingInvaders = CalcParticipatingInvaders();
			while(remainingDamageToApply > 0 && participatingInvaders.Any()) {
				Token invadertodamage = PickSmartInvaderToDamage( participatingInvaders, remainingDamageToApply );
				remainingDamageToApply -= await grp.ApplyDamageTo1( remainingDamageToApply, invadertodamage );
				// update participants
				participatingInvaders = CalcParticipatingInvaders();
			}
			tracking.endingInvaders = grp.Tokens.InvaderSummary;
		}

		#region private

		int RawDamageFromParticipatingInvaders( CountDictionary<Token> participatingInvaders ) {

			return participatingInvaders.Keys
				.Where( x => !(x is StrifedInvader) )
				.Select( invader => invader.FullHealth * Tokens[invader] ).Sum();
		}


		void FromEachStrifed_RemoveOneStrife( CountDictionary<Token> participatingInvaders ) {
			var strifed = participatingInvaders.Keys.OfType<StrifedInvader>()
				.OrderBy( x => x.StrifeCount ) // smallest first
				.ToArray();
			foreach(var orig in strifed)
				Tokens.RemoveStrife( orig, Tokens[orig] );
		}

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
