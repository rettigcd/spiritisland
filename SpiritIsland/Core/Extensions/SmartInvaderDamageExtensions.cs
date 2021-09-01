using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	static public class SmartInvaderDamageExtensions {

		//  This is the replacement for SmartDamage To Types
		static public async Task<int> UserSelectDamage( this IMakeGamestateDecisions ctx, int damage, InvaderGroup group ) {
			while(damage > 0) {
				var invader = await ctx.Self.Action.Choose( new SelectInvaderToDamage( damage, group.Space, group.Counts.Keys.ToArray(), Present.Always ) );
				if(invader == null) break;

				damage -= await group.ApplyDamageTo1( damage, invader );
			}

			return damage;
		}

		public static InvaderSpecific PickSmartInvaderToDamage( this IInvaderCounts counts, int availableDamage, params Invader[] invaderGeneric ) { // $$
			return SmartInvaderAttacker.Singleton.GetKillOrder( counts, availableDamage, invaderGeneric );
		}

		// When we swap this out for user choosing, Which user chooses when dahan are doing damage????
		static public async Task ApplySmartDamageToGroup( this InvaderGroup grp, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.Counts.Keys.Any()) {
				InvaderSpecific invaderToDamage = grp.Counts.PickSmartInvaderToDamage( damageToInvaders );
				damageToInvaders -= await grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {grp}." );
		}

		static async public Task SmartDamageToTypes( this InvaderGroup grp, int startingDamage, params Invader[] invaderGeneric ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			IEnumerable<InvaderSpecific> Targets() => grp.Counts.Keys.Where(k=> invaderGeneric.Contains(k.Generic));

			while(damageToInvaders > 0 && Targets().Any()) {
				InvaderSpecific invaderToDamage = grp.Counts.PickSmartInvaderToDamage( damageToInvaders, invaderGeneric );
				damageToInvaders -= await grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
		}

		static public InvaderSpecific PickBestInvaderToRemove( this IInvaderCounts counts, params Invader[] removable ) {
			return counts.Keys
				.Where(k=>removable.Contains(k.Generic))
				.OrderByDescending( g => g.FullHealth )
				.ThenByDescending( g => g.Health )
				.First();
		}

		// pics the best one to remove
		static public void Remove( this IInvaderCounts counts, Invader generic, int numToRemove = 1 ) {
			if(numToRemove<0) throw new ArgumentOutOfRangeException(nameof(numToRemove));
			var specific = counts.Keys.Where(k=>k.Generic==generic).OrderByDescending(x=>x.Health).FirstOrDefault();
			if(specific != null)
				counts[specific] -= numToRemove;
		}

	}

	public class SmartInvaderAttacker {

		static public SmartInvaderAttacker Singleton = new SmartInvaderAttacker();

		public InvaderSpecific GetKillOrder( 
			IInvaderCounts counts, 
			int availableDamage
			, params Invader[] invaderGeneric
		) {
			var candidates = counts.Keys;
			if(invaderGeneric != null && invaderGeneric.Length > 0)
				candidates = candidates.Where( i => invaderGeneric.Contains( i.Generic ) );

			return PickItemToKill( candidates, availableDamage )
				?? PickItemToDamage( candidates );
		}

		InvaderSpecific PickItemToKill(IEnumerable<InvaderSpecific> candidates, int availableDamage) {
			return candidates
				.Where( specific => specific.Health <= availableDamage ) // can be killed
				.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
				.ThenBy( k => k.Health ) // and most damaged.
				.FirstOrDefault();
		}

		InvaderSpecific PickItemToDamage( IEnumerable<InvaderSpecific> candidates ) {
			return candidates
				.OrderBy(i=>i.Health) // closest to dead
				.ThenByDescending(i=>i.FullHealth) // biggest impact
				.First();
		}

		#region private
		readonly InvaderSpecific[] LeftOverOrder;
		#endregion

	}

}