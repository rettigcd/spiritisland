using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {
	static public class SmartInvaderDamageExtensions {


		// !! This is the replacement for SmartDamage To Types
		static public async Task<int> UserSelectDamage( this ActionEngine engine, int damage, InvaderGroup group ) {
			while(damage > 0) {
				var invader = await engine.SelectInvader( "Select invader to damage.", group.InvaderTypesPresent.ToArray() );
				if(invader == null) break;

				damage -= group.ApplyDamageTo1( damage, invader );
			}

			return damage;
		}




		static readonly SmartInvaderKiller InvaderPicker = new SmartInvaderKiller();
		public static InvaderSpecific PickSmartInvaderToDamage( this InvaderGroup grp, int availableDamage ) {
			return InvaderPicker.PickOne( grp, availableDamage );
		}

		// !!! When we swap this out for user choosing, Which user chooses when dahan are doing damage????
		static public Task SmartDamageToGroup( this InvaderGroup grp, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.InvaderTypesPresent.Any()) {
				InvaderSpecific invaderToDamage = PickSmartInvaderToDamage( grp, damageToInvaders );
				damageToInvaders -= grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {grp}." );

			return Task.CompletedTask;
		}

		static public Task SmartDamageToTypes( this InvaderGroup grp, int startingDamage, params Invader[] healthyInvaders ) {
			int damageToInvaders = startingDamage;

			var allTargetTypes = healthyInvaders.SelectMany( x => x.AliveVariations ).ToArray();

			var picker = new SmartInvaderKiller();
			picker.LimitTo( allTargetTypes );

			// While damage remains    &&    we have invaders
			IEnumerable<InvaderSpecific> Targets() => grp.InvaderTypesPresent.Intersect( allTargetTypes );

			while(damageToInvaders > 0 && Targets().Any()) {
				InvaderSpecific invaderToDamage = picker.PickOne( grp, damageToInvaders );
				damageToInvaders -= grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}

			return Task.CompletedTask;
		}

		class SmartInvaderKiller {
			public InvaderSpecific[] KillOrder = "C@1 C@2 C@3 T@1 T@2 E@1".Split( ' ' ).Select( k => InvaderSpecific.Lookup[k] ).ToArray();
			public InvaderSpecific[] LeftOverOrder = "C@2 T@2 C@3".Split( ' ' ).Select( k => InvaderSpecific.Lookup[k] ).ToArray();

			public void LimitTo( InvaderSpecific[] types ) {
				KillOrder = KillOrder.Where( x => types.Contains( x ) ).ToArray();
				LeftOverOrder = LeftOverOrder.Where( x => types.Contains( x ) ).ToArray();
			}

			public InvaderSpecific PickOne( InvaderGroup grp, int availableDamage ) {
				return KillOrder
				.FirstOrDefault( invader =>
					invader.Health <= availableDamage // prefer things we can kill
					&& grp[invader] > 0
				)
				?? LeftOverOrder.First( invader => grp[invader] > 0 ); // left-over damage
			}

		}


	}

}