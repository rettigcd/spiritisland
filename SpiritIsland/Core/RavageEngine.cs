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
			await eng.DamageLand( damageInflictedFromInvaders );
			await eng.DamageDahan( damageInflictedFromInvaders );
			int damageFromDahan = eng.GetDamageInflictedByDahan();
			await eng.DamageInvaders( damageFromDahan );
		}

		bool HasInvaders => grp.Tokens.HasInvaders();

		public int GetDamageInflictedByDahan() => gs.DahanGetCount( grp.Space ) * 2;

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
			foreach(var orig in strifed) {
				var lessStrifed = orig.AddStrife( -1 );
				Counts[lessStrifed] += Counts[orig];
				Counts[orig] = 0;
			}
		}

		public async Task DamageLand( int damageInflictedFromInvaders ) {
			if( cfg.ShouldDamageLand && damageInflictedFromInvaders > 1) {
				await gs.BlightLand( grp.Space );
				log.Add( "Blights land." );
			}
		}

		/// <returns># of dahan killed/destroyed</returns>
		public async Task<int> DamageDahan( int damageInflictedFromInvaders ) {
			if(damageInflictedFromInvaders == 0 || !cfg.ShouldDamageDahan) return 0;

			// destroy dahan
			int dahanOnSpace = gs.DahanGetCount( grp.Space );
			int dahanDestroyed = Math.Min( damageInflictedFromInvaders / cfg.DahanHitpoints, dahanOnSpace ); // rounding down
			if(dahanDestroyed != 0) {
				await gs.DestroyToken( grp.Space, dahanDestroyed, TokenType.Dahan.Default, Cause.Invaders );
			}

			int leftOverDamage = damageInflictedFromInvaders - dahanDestroyed * cfg.DahanHitpoints;
			bool convert1To1HitPoint = leftOverDamage == cfg.DahanHitpoints - 1;
			if(convert1To1HitPoint && gs.DahanGetCount( grp.Space )>0) {
				Counts[TokenType.Dahan[2]]--;
				Counts[TokenType.Dahan[1]]++;
			}

			log.Add( $"Kills {dahanDestroyed} of {dahanOnSpace} Dahan leaving {dahanOnSpace - dahanDestroyed} Dahan." );
			return dahanDestroyed;
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
