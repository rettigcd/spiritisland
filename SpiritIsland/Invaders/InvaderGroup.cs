using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvaderGroup {

		#region constructor

		public InvaderGroup( TokenCountDictionary aliveCounts) {
			this.Tokens = aliveCounts;
		}

		#endregion

		public Space Space => Tokens.Space;

		public int this[Token specific] { 
			get{ return Tokens[specific]; }
			set{ Tokens[specific] = value; }
		}

		#region Extension Methods to Pull off

		public int DamageInflictedByInvaders => Tokens.Invaders().Select( invader => invader.FullHealth * this[invader] ).Sum();

		public async Task ApplyDamageToEach( int individualDamage, params TokenGroup[] generic ) {
			foreach(var invader in Tokens.Invaders())
				if(generic.Contains(invader.Generic))
					await ApplyDamageToAllTokensOfType( individualDamage, invader );
		}

		public async Task ApplyDamageToAllTokensOfType( int individualDamage, IEnumerable<Token> part ) {
			var ordered = part.OrderByDescending( x => x.Health );// MUST damage healthy first so we don't double damage 
			foreach(var specific in ordered)
				await ApplyDamageToAllTokensOfType(individualDamage,specific);
		}

		async public Task ApplyDamageToAllTokensOfType( int individualDamage, Token token ) {
			while(this[token] > 0)
				await ApplyDamageTo1( individualDamage, token );
		}

		public async Task<int> Destroy( int countToDestory, TokenGroup generic ) {
			if(countToDestory == 0) return 0;
			Token[] invaderTypesToDestory = Tokens.Invaders()
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
			Token[] invadersToDestroy = Tokens.OfAnyType( generics );
			while(count > 0 && invadersToDestroy.Length > 0) {
				var invader = invadersToDestroy
					.OrderByDescending(x=>x.FullHealth)
					// .ThenByDescending(x=>x.Health) assume this line is in Destroy(...)
					.First();
				await Destroy( 1, invader.Generic );

				// next
				invadersToDestroy = Tokens.OfAnyType( generics );
				--count;
			}
		}

		/// <returns>damage inflicted to invaders</returns>
		public async Task<int> ApplyDamageTo1( int availableDamage, Token invader ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.ResultingDamagedInvader( damageToInvader );
			this.
			Tokens.Adjust( invader, -1 );

			if(damagedInvader.Health > 0) // don't track dead invaders
				Tokens.Adjust( damagedInvader, 1 );

			if(damagedInvader.Health == 0)
				await DestroyInvaderStrategy.OnInvaderDestroyed( Space, damagedInvader );
			return damageToInvader;
		}

		#endregion

		public DestroyInvaderStrategy DestroyInvaderStrategy; // This will be null for Building
		public readonly TokenCountDictionary Tokens;

		static public void HealTokens( TokenCountDictionary counts ) {
			void Heal( Token damaged ) {
				int num = counts[damaged];
				counts.Adjust( damaged.Healthy, num );
				counts.Adjust( damaged, -num );
			}

			void HealGroup( TokenGroup group ) {
				for(int health = group.Default.Health - 1; health > 0; --health)
					Heal( group[health] );
			}

			HealGroup( Invader.City );
			HealGroup( Invader.Town );
			HealGroup( TokenType.Dahan );
		}

	}

}
