using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ConfigureRavage {

		public bool ShouldDamageLand { get; set; } = true;
		public bool ShouldDamageDahan { get; set; } = true;

		public int DahanHitpoints { get; set; } = 2;

		public CountDictionary<Token> NotParticipating {  get; set; } = new CountDictionary<Token>();

		public Func<RavageEngine, Task> RavageSequence = null; // null triggers default
	}

	public class RavageEngine {

		public List<string> log = new List<String>();
		public int DahanDestroyed { get; private set; }

	
		public TokenCountDictionary Counts => grp.Tokens;

		readonly protected InvaderGroup grp;
		readonly protected GameState gs;
		readonly protected ConfigureRavage cfg;

		protected int Defend => Counts.Defend;

		public RavageEngine(GameState gs,InvaderGroup grp, ConfigureRavage cfg ) {
			this.gs = gs;
			this.grp = grp;
			this.cfg = cfg;
		}

		public async Task Exec() {
			if(HasInvaders)
				await (cfg.RavageSequence ?? DefaultRavageSequence)( this);
		}

		static async Task DefaultRavageSequence( RavageEngine eng ) {
			int damageInflictedFromInvaders = eng.GetDamageInflictedByInvaders();
			await eng.DamageDahan( damageInflictedFromInvaders );
			int damageFromDahan = eng.GetDamageInflictedByDahan();
			await eng.DamageInvaders( damageFromDahan );
			// worry about this last - why?, just because
			await eng.DamageLand( damageInflictedFromInvaders );
		}

		bool HasInvaders => grp.Tokens.HasInvaders();

		public int GetDamageInflictedByDahan() => gs.DahanOn( grp.Space ).Count * 2;

		public CountDictionary<Token> CalcParticipatingInvaders() { 
			var participants = new CountDictionary<Token>();
			foreach(var token in Counts.Invaders()) {
				participants[token] = Math.Max(0,Counts[token] - cfg.NotParticipating[token]);
			}
			return participants;
		}

		public int GetDamageInflictedByInvaders() {

			var participatingInvaders = CalcParticipatingInvaders();

			int damageFromInvaders = RawDamageFromParticipatingInvaders( participatingInvaders );

			FromEachStrifed_RemoveOneStrife( participatingInvaders );

			// Defend
			int damageInflictedFromInvaders = Math.Max( damageFromInvaders - Defend, 0 );

			return damageInflictedFromInvaders;
		}

		int RawDamageFromParticipatingInvaders( CountDictionary<Token> participatingInvaders ) {

			return participatingInvaders.Keys
				.Where( x => !(x is StrifedInvader) )
				.Select( invader => invader.FullHealth * Counts[invader] ).Sum();
		}

		void FromEachStrifed_RemoveOneStrife( CountDictionary<Token> participatingInvaders ) {
			var strifed = participatingInvaders.Keys.OfType<StrifedInvader>()
				.OrderBy( x => x.StrifeCount ) // smallest first
				.ToArray();
			foreach(var orig in strifed)
				Counts.RemoveStrife( orig, Counts[orig] );
		}

		public async Task DamageLand( int damageInflictedFromInvaders ) {
			if( cfg.ShouldDamageLand )
				await gs.DamageLand(grp.Space,damageInflictedFromInvaders);
		}

		/// <returns># of dahan killed/destroyed</returns>
		public async Task DamageDahan( int damageInflictedFromInvaders ) {
			if(damageInflictedFromInvaders == 0 || !cfg.ShouldDamageDahan) return;

			// ! This special DamageDahan, uses the config to change dahan health points.

			// destroy dahan
			var dahan = grp.Tokens.Dahan;
			int dahanOnSpace = dahan.Count;
			int dahanDestroyed = Math.Min( damageInflictedFromInvaders / cfg.DahanHitpoints, dahanOnSpace ); // rounding down
			if(dahanDestroyed != 0)
				await dahan.Destroy( dahanDestroyed,2, Cause.Invaders );

			int leftOverDamage = damageInflictedFromInvaders - dahanDestroyed * cfg.DahanHitpoints;
			bool convert1To1HitPoint = leftOverDamage == cfg.DahanHitpoints - 1;
			if(convert1To1HitPoint && dahan.Any)
				await dahan.ApplyDamage(1,Cause.Invaders);

			log.Add( $"Kills {dahanDestroyed} of {dahanOnSpace} Dahan leaving {dahanOnSpace - dahanDestroyed} Dahan." );
		}

		/// <returns>(city-dead,town-dead,explorer-dead)</returns>
		public async Task DamageInvaders( int damageFromDahan ) {
			int damagetoinvaders = damageFromDahan;

			var participatingInvaders = CalcParticipatingInvaders();
			while(damagetoinvaders > 0 && participatingInvaders.Any()) {
				Token invadertodamage = PickSmartInvaderToDamage( participatingInvaders, damagetoinvaders );
				damagetoinvaders -= await grp.ApplyDamageTo1( damagetoinvaders, invadertodamage );
				// update participants
				participatingInvaders = CalcParticipatingInvaders();
			}
			if(log != null) log.Add( $"{damageFromDahan} damage to invaders leaving {grp.Tokens.InvaderSummary}." );
		}

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

	}


}
