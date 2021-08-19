using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvaderGroup : InvaderGroup_Readonly {

		#region constructor

		public InvaderGroup( Space space, int[] counts, Action<int> addFear )
			: base( space, counts ) {
			this.addFear = addFear;
		}

		#endregion

		public async Task ApplyDamageToEach( int individualDamage, params Invader[] invaders ) {
			foreach(Invader invader in invaders)
				foreach(var part in invader.AliveVariations)
					while(this[part] > 0)
						await ApplyDamageTo1( individualDamage, part );
		}

		public async Task<int> Destroy( Invader generic, int countToDestory ) {
			if(countToDestory == 0) return 0;
			InvaderSpecific[] invaderTypesToDestory = generic.AliveVariations
				.Where( specific => this[specific] > 0 )
				.OrderByDescending( x => x.Health ) // kill healthiest first
				.ToArray();

			int totalDestoyed = 0;
			foreach(var specificInvader in invaderTypesToDestory) {
				while(countToDestory > 0 && this[specificInvader] > 0) {
					await ApplyDamageTo1( specificInvader.Health, specificInvader );
					++totalDestoyed;
					--countToDestory;
				}
			}
			return totalDestoyed;
		}

		/// <returns>damage inflicted to invaders</returns>
		public async Task<int> ApplyDamageTo1( int availableDamage, InvaderSpecific invader ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			Adjust(invader,-1);
			Adjust(damagedInvader,1);

			if(damagedInvader.Health == 0)
				await OnInvaderDestroyed(damagedInvader);
			return damageToInvader;
		}

		public void Heal() {

			void Heal( InvaderSpecific damaged ) {
				int num = this[damaged];
				Adjust(damaged.Healthy, num );
				Adjust(damaged, -num);
			}

			Heal( InvaderSpecific.City1 );
			Heal( InvaderSpecific.City2 );
			Heal( InvaderSpecific.Town1 );
		}

		protected virtual Task OnInvaderDestroyed( InvaderSpecific specific ) {
			if(specific == InvaderSpecific.City0)
				addFear( 2 );
			if(specific == InvaderSpecific.Town0)
				addFear( 1 );
			return Task.CompletedTask;
		}

		protected readonly Action<int> addFear;

	}

}
