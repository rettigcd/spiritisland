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

		public async Task UserSelectedDamage( int damage, Spirit damagePicker, params TokenGroup[] allowedTypes ) {
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

		static readonly DamageApplier DefaultDamageApplicationStrategy = new DamageApplier();
		class DamageApplier : IDamageApplier {
			Token IDamageApplier.ApplyDamage( TokenCountDictionary tokens, int availableDamage, Token invaderToken ) {
				var damagedInvader = invaderToken.ResultingDamagedInvader( availableDamage );
				tokens.Adjust( invaderToken, -1 );
				if(0 < damagedInvader.Health) // only track alive invaders
					tokens.Adjust( damagedInvader, 1 );
				return damagedInvader;
			}
		}

	}

	// Allows to intercept applying specific damage (Flame's Fury)
	public interface IDamageApplier {
		Token ApplyDamage( TokenCountDictionary tokens, int availableDamage, Token invaderToken );
	}

}
