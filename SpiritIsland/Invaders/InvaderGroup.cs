using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvaderGroup {

		#region constructor

		public InvaderGroup( Space space, TokenCountDictionary aliveCounts) {
			this.Counts = aliveCounts;
			this.Space = space;
		}

		#endregion

		public Space Space { get; }

		public int this[Token specific] { 
			get{ return Counts[specific]; }
			set{ Counts[specific] = value; }
		}

		public override string ToString()
			=> Counts.ToSummary();

		#region Extension Methods to Pull off

		public int DamageInflictedByInvaders => Counts.Invaders().Select( invader => invader.FullHealth * this[invader] ).Sum();

		public async Task ApplyDamageToEach( int individualDamage, params TokenGroup[] generic ) {
			// !!! this is wrong - only applies damage to single item
			foreach(var invader in Counts.Invaders())
				if(generic.Contains(invader.Generic))
					await ApplyDamageToSpecific( individualDamage, invader );
		}

		public async Task ApplyDamageToSpecifics( int individualDamage, IEnumerable<Token> part ) {
			var ordered = part.OrderByDescending( x => x.Health );// MUST damage healthy first so we don't double damage 
			foreach(var specific in ordered)
				await ApplyDamageToSpecific(individualDamage,specific);
		}

		async Task ApplyDamageToSpecific( int individualDamage, Token token ) {
			while(this[token] > 0)
				await ApplyDamageTo1( individualDamage, token );
		}

		public async Task<int> Destroy( int countToDestory, TokenGroup generic ) {
			if(countToDestory == 0) return 0;
			Token[] invaderTypesToDestory = Counts.Invaders()
				.Where( x=> x.Generic==generic )
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

		public async Task<int> Destroy( int countToDestory, Token specificInvader ) {
			if(countToDestory == 0) return 0;
			int numToDestory = Math.Min(countToDestory, this[specificInvader] );
			for(int i = 0; i < numToDestory; ++i)
				await ApplyDamageTo1( specificInvader.Health, specificInvader );
			return numToDestory;
		}

		public async Task DestroyAny( int count, params TokenGroup[] generics ) {
			// !! this could be cleaned up
			Token[] invadersToDestroy = Counts.OfAnyType( generics );
			while(count > 0 && invadersToDestroy.Length > 0) {
				var invader = invadersToDestroy
					.OrderByDescending(x=>x.FullHealth)
					// .ThenByDescending(x=>x.Health) assume this line is in Destroy(...)
					.First();
				await Destroy( 1, invader.Generic );

				// next
				invadersToDestroy = Counts.OfAnyType( generics );
				--count;
			}
		}

		#endregion

		/// <returns>damage inflicted to invaders</returns>
		public async Task<int> ApplyDamageTo1( int availableDamage, Token invader ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			Counts.Adjust(invader,-1);
			if(damagedInvader.Health>0) // don't track dead invaders
				Counts.Adjust(damagedInvader,1);

			if(damagedInvader.Health == 0)
				await OnInvaderDestroyed(damagedInvader);
			return damageToInvader;
		}

		public void Heal() {

			void Heal( Token damaged ) {
				int num = this[damaged];
				Counts.Adjust(damaged.Healthy, num );
				Counts.Adjust(damaged, -num);
			}

			Heal( Invader.City[1] );
			Heal( Invader.City[2] );
			Heal( Invader.Town[1] );
			Heal( TokenType.Dahan[1] );
		}

		protected virtual Task OnInvaderDestroyed( Token specific ) {
			return DestroyInvaderStrategy.OnInvaderDestroyed(Space,specific);
		}

		public DestroyInvaderStrategy DestroyInvaderStrategy; // This will be null for Building
		public readonly TokenCountDictionary Counts;

	}

	public class DestroyInvaderStrategy {

		public DestroyInvaderStrategy( Action<FearArgs> addFear, Cause destructionSource ) {
			if(destructionSource == Cause.None) 
				throw new ArgumentException("if we are destroying things, there must be a cause");
			this.addFear = addFear;
			this.destructionSource = destructionSource;
		}

		public virtual Task OnInvaderDestroyed( Space space, Token specific ) {
			if(specific == Invader.City[0])
				AddFear( space, 2 );
			if(specific == Invader.Town[0])
				AddFear( space, 1 );
			return Task.CompletedTask;
		}

		protected void AddFear( Space space, int count ) => addFear( new FearArgs { count = count, cause = this.destructionSource, space = space } );

		readonly Action<FearArgs> addFear;
		readonly Cause destructionSource;

	}

}
