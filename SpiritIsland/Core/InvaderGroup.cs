using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvaderGroup : InvaderGroup_Readonly {

		#region constructor

		public InvaderGroup( Space space, int[] counts, Action<FearArgs> addFear, Cause dSource )
			: base( space, counts ) {
			this.addFear = addFear;
			destructionSource = dSource;
		}

		#endregion

		protected readonly Cause destructionSource;

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

		public async Task<int> Destroy( int countToDestory, InvaderSpecific specificInvader ) {
			if(countToDestory == 0) return 0;
			int numToDestory = Math.Min(countToDestory, this[specificInvader] );
			for(int i = 0; i < numToDestory; ++i)
				await ApplyDamageTo1( specificInvader.Health, specificInvader );
			return numToDestory;
		}


		public async Task DestroyAny( int count, params Invader[] generics ) {
			// !! this could be cleaned up
			InvaderSpecific[] invadersToDestroy = FilterBy( generics );
			while(count > 0 && invadersToDestroy.Length > 0) {
				var invader = invadersToDestroy
					.OrderByDescending(x=>x.Healthy.Health)
					// .ThenByDescending(x=>x.Health) assume this line is in Destroy(...)
					.First();
				await Destroy( invader.Generic, 1 );

				// next
				invadersToDestroy = FilterBy( generics );
				--count;
			}
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
				AddFear( 2 );
			if(specific == InvaderSpecific.Town0)
				AddFear( 1 );
			return Task.CompletedTask;
		}

		protected void AddFear(int count) => addFear( new FearArgs { count = count, cause = this.destructionSource, space = Space } );

		protected readonly Action<FearArgs> addFear;

	}

}
