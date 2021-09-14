using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	static public class SmartInvaderDamageExtensions {

		//  This is the replacement for SmartDamage To Types
		static public async Task<int> UserSelectDamage( this SpiritGameStateCtx ctx, int damage, InvaderGroup group ) {
			while(damage > 0) {
				var invader = await ctx.Self.Action.Decision( new Decision.InvaderToDamage( damage, group.Space, group.Tokens.Invaders(), Present.Always ) );
				if(invader == null) break;

				damage -= await group.ApplyDamageTo1( damage, invader );
			}

			return damage;
		}

		public static Token PickSmartInvaderToDamage( this TokenCountDictionary counts, int availableDamage, params TokenGroup[] invaderGeneric ) { // $$
			return SmartInvaderAttacker.Singleton.GetKillOrder( counts, availableDamage, invaderGeneric );
		}

		// When we swap this out for user choosing, Which user chooses when dahan are doing damage????
		static public async Task SmartDamageToGroup( this InvaderGroup grp, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.Tokens.HasInvaders()) {
				Token invaderToDamage = grp.Tokens.PickSmartInvaderToDamage( damageToInvaders );
				damageToInvaders -= await grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {grp}." );
		}

		static public void SmartRemovalOfHealth( this InvaderGroup grp, int startingDamage ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.Tokens.HasInvaders()) {
				Token invaderToDamage = grp.Tokens.PickSmartInvaderToDamage( damageToInvaders );
				if(invaderToDamage.Health < damageToInvaders) {
					--grp.Tokens[invaderToDamage];
					damageToInvaders -= invaderToDamage.Health;
				} else {
					break;
				}
			}
		}

		// Stolen from Emigration Accelerates
		static public void RemoveInvader( this TokenCountDictionary grp, params TokenGroup[] removable ) {
			if(grp.SumAny(removable) == 0) return;
			var invaderToRemove = grp.PickBestInvaderToRemove( removable );
			grp.Adjust( invaderToRemove, -1 );
		}


		static async public Task SmartDamageToTypes( this InvaderGroup grp, int startingDamage, params TokenGroup[] invaderGenerics ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			IEnumerable<Token> Targets() => grp.Tokens.OfAnyType(invaderGenerics);

			while(damageToInvaders > 0 && Targets().Any()) {
				Token invaderToDamage = grp.Tokens.PickSmartInvaderToDamage( damageToInvaders, invaderGenerics );
				damageToInvaders -= await grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
		}

		static public Token PickBestInvaderToRemove( this TokenCountDictionary counts, params TokenGroup[] removables ) {
			return counts.OfAnyType( removables )
				.OrderByDescending( g => g.FullHealth )
				.ThenByDescending( g => g.Health )
				.First();
		}

		// pics the best one to remove
		static public void Remove( this TokenCountDictionary counts, TokenGroup generic, int numToRemove = 1 ) {
			if(numToRemove<0) throw new ArgumentOutOfRangeException(nameof(numToRemove));
			var specific = counts.OfType(generic).OrderByDescending(x=>x.Health).FirstOrDefault();
			if(specific != null)
				counts[specific] -= numToRemove;
		}

	}

	public class SmartInvaderAttacker {

		static readonly public SmartInvaderAttacker Singleton = new SmartInvaderAttacker();

		public Token GetKillOrder(  TokenCountDictionary counts,  int availableDamage , params TokenGroup[] invaderGeneric ) {
			var candidates = counts.Invaders();
			if(invaderGeneric != null && invaderGeneric.Length > 0)
				candidates = candidates.Where( i => invaderGeneric.Contains( i.Generic ) );

			return PickItemToKill( candidates, availableDamage )
				?? PickItemToDamage( candidates );
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