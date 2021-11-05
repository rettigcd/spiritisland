using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvaderGroup {

		#region constructor

		public InvaderGroup( TokenCountDictionary aliveCounts, DestroyInvaderStrategy destoryStrategy, IDamageApplier customeDamageApplicationStrategy = null) {
			this.Tokens = aliveCounts;
			this.damageApplicationStrategy = customeDamageApplicationStrategy ?? DefaultDamageApplicationStrategy;
			this.DestroyStrategy = destoryStrategy;
		}

		#endregion

		public Space Space => Tokens.Space;

		#region Read-only
		public int this[Token specific] => Tokens[specific];

		public int DamageInflictedByInvaders => Tokens.Invaders().Select( invader => invader.FullHealth * this[invader] ).Sum();

		#endregion

		#region Damage

		public async Task ApplyDamageToEach( int individualDamage, params TokenGroup[] generic ) {

			var invaders = Tokens.Invaders()
				.OrderBy(x=>x.Health) // do damaged first to clear them out
				.ToArray();

			// Filter if appropriate
			if(generic != null && generic.Length>0)
				invaders = invaders.Where(t=>generic.Contains(t.Generic)).ToArray();

			foreach(var invader in invaders)
				while(this[invader] > 0)
					await ApplyDamageTo1( individualDamage, invader );

		}

		/// <returns>damage inflicted to invaders</returns>
		public async Task<int> ApplyDamageTo1( int availableDamage, Token invaderToken ) {

			Token damagedInvader = damageApplicationStrategy.ApplyDamage( Tokens, availableDamage, invaderToken );

			if(0 == damagedInvader.Health)
				await DestroyStrategy.OnInvaderDestroyed( Space, damagedInvader );

			return invaderToken.Health - damagedInvader.Health; // damage inflicted
		}

		readonly IDamageApplier damageApplicationStrategy;

		#endregion

		#region Destroy

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

		#endregion Destroy

		#region Remove

		//public void Remove( TokenGroup group ) {
		//	// there is no need to order by .Generic.Health because there is only 1 TokenGroup
		//	var invaderToRemove = Tokens.OfType( group )
		//		.OrderBy( k => k.Strife() )  // un-strifed first
		//		.ThenByDescending( x=>x.Health ) // secondary, worry about current health
		//		.FirstOrDefault();
		//	if(invaderToRemove != null)
		//		--Tokens[invaderToRemove];
		//}

		/// <remarks>
		/// This is neither damage nor destory.
		/// It is Game-Aware in that it understands non-strifed invaders are more dangerous than non-strifed, so it doesn't belong in the generic TokenDictionary class.
		/// However, it also does not require any input from a user, so it should not be on a TargetSpaceCtx.
		/// Sticking on InvaderGroup is the only place I can think to put it.
		/// Also, shouldn't be affected by Bringer overwriting 'Destroy' and 'Damage'
		/// </remarks>
		public void Remove( params TokenGroup[] removables ) {
			if(Tokens.SumAny(removables) == 0) return;

			var invaderToRemove = Tokens.OfAnyType( removables )
				.OrderByDescending( g => g.FullHealth )
				.ThenBy( k => k.Strife() )  // un-strifed first
				.ThenByDescending( g => g.Health )
				.FirstOrDefault();

			if(invaderToRemove != null)
				Tokens.Adjust( invaderToRemove, -1 );
		}

		#endregion

		/// <summary>
		/// Restores all of the tokens to their default / healthy state.
		/// </summary>
		static public void HealTokens( TokenCountDictionary counts ) {
			void RestoreAllToDefault( Token token ) {
				if(token == token.Healthy) return; // already at default/healthy
				int num = counts[token];
				counts.Adjust( token.Healthy, num );
				counts.Adjust( token, -num );
			}

			void HealGroup( TokenGroup group ) {
				foreach(var token in counts.OfType(group).ToArray())
					RestoreAllToDefault(token);
			}

			HealGroup( Invader.City );
			HealGroup( Invader.Town );
			HealGroup( TokenType.Dahan );
		}

		public async Task UserSelectedDamage( int damage, Spirit damagePicker, params TokenGroup[] allowedTypes ) {
			if(damage == 0) return;
			if(allowedTypes == null || allowedTypes.Length == 0)
				allowedTypes = new TokenGroup[] { Invader.Explorer, Invader.Town, Invader.City };

			Token[] invaderTokens;
			while(damage>0 && (invaderTokens=Tokens.OfAnyType(allowedTypes).ToArray()).Length > 0) {
				var invaderToDamage = await damagePicker.Action.Decision(new Decision.TokenOnSpace($"Damage ({damage} remaining)",Space,invaderTokens,Present.Always ));
				await ApplyDamageTo1(1,invaderToDamage);
				damage--;
			}
		}

		public readonly DestroyInvaderStrategy DestroyStrategy;
		public readonly TokenCountDictionary Tokens;

		static readonly DefaultDamageApplier DefaultDamageApplicationStrategy = new DefaultDamageApplier();

	}


}
