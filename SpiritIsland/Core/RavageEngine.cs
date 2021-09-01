using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ConfigureRavage {

		public bool ShouldRavage { get; set; } = true;
		public bool ShouldDamageLand { get; set; } = true;
		public bool ShouldDamageDahan { get; set; } = true;

		public int DahanHitpoints { get; set; } = 2;
		public int Defend { get; set; } = 0;
		public Func<RavageEngine, Task> RavageSequence = null; // null triggers default
	}

	public class RavageEngine {

		public List<string> log = new List<String>();
		public int DahanDestroyed { get; private set; }
		
		public IInvaderCounts Counts => grp.Counts;

		readonly protected InvaderGroup grp;
		readonly protected GameState gs;
		readonly protected ConfigureRavage cfg;

		public RavageEngine(GameState gs,InvaderGroup grp, ConfigureRavage cfg ) {
			this.gs = gs;
			this.grp = grp;
			this.cfg = cfg;
		}

		public async Task Exec() {
			if(cfg.ShouldRavage && HasInvaders)
				await (cfg.RavageSequence ?? DefaultRavageSequence)( this);
		}

		static async Task DefaultRavageSequence( RavageEngine eng ) {
			int damageInflictedFromInvaders = eng.GetDamageInflictedByInvaders();
			await eng.DamageLand( damageInflictedFromInvaders );
			await eng.DamageDahan( damageInflictedFromInvaders );
			int damageFromDahan = eng.GetDamageInflictedByDahan();
			await eng.DamageInvaders( damageFromDahan );
		}

		bool HasInvaders => grp.Counts.Keys.Any();

		public int GetDamageInflictedByDahan() => gs.Dahan.Count( grp.Space ) * 2;

		public virtual int GetDamageInflictedByInvaders() {
			int damageFromInvaders = grp.DamageInflictedByInvaders;
			int damageInflictedFromInvaders = Math.Max( damageFromInvaders - cfg.Defend, 0 );

			if(damageInflictedFromInvaders == 0)
				log.Add( "no damage from invaders" );
			else
				log.Add( $"{grp} inflicts {damageFromInvaders}-{cfg.Defend}={damageInflictedFromInvaders} damage." );

			return damageInflictedFromInvaders;
		}

		public async Task DamageLand( int damageInflictedFromInvaders ) {
			if( cfg.ShouldDamageLand && damageInflictedFromInvaders > 1) {
				await gs.BlightLand( grp.Space );
				log.Add( "Blights land." );
			}
		}

		/// <returns># of dahan killed/destroyed</returns>
		public async Task<int> DamageDahan(int damageInflictedFromInvaders ) {
			if(damageInflictedFromInvaders == 0 || !cfg.ShouldDamageDahan) return 0;

			int dahanOnSpace = gs.Dahan.Count( grp.Space );
			int dahanDestroyed = Math.Min( damageInflictedFromInvaders / cfg.DahanHitpoints, dahanOnSpace ); // rounding down
			if(dahanDestroyed == 0) return 0;

			await gs.Dahan.Destroy( grp.Space, dahanDestroyed, Cause.Invaders );
			log.Add( $"Kills {dahanDestroyed} of {dahanOnSpace} Dahan leaving {dahanOnSpace - dahanDestroyed} Dahan." );
			return dahanDestroyed;
		}

		/// <returns>(city-dead,town-dead,explorer-dead)</returns>
		public async Task DamageInvaders( int damageFromDahan ) {
			await grp.ApplySmartDamageToGroup( damageFromDahan, log );
		}
	}


}
