﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {
	static public class SmartInvaderDamageExtensions {


		// !! This is the replacement for SmartDamage To Types
		static public async Task<int> UserSelectDamage( this ActionEngine engine, int damage, InvaderGroup group ) {
			while(damage > 0) {
				var invader = await engine.SelectInvader( "Select invader to damage.", group.InvaderTypesPresent_Specific.ToArray() );
				if(invader == null) break;

				damage -= group.ApplyDamageTo1( damage, invader );
			}

			return damage;
		}


		public static InvaderSpecific PickSmartInvaderToDamage( this InvaderGroup grp, int availableDamage ) {
			return SmartInvaderAttacker.GetKillOrder( grp, availableDamage ).First();
		}

		// !!! When we swap this out for user choosing, Which user chooses when dahan are doing damage????
		static public Task ApplySmartDamageToGroup( this InvaderGroup grp, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.InvaderTypesPresent_Specific.Any()) {
				InvaderSpecific invaderToDamage = PickSmartInvaderToDamage( grp, damageToInvaders );
				damageToInvaders -= grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {grp}." );

			return Task.CompletedTask;
		}

		static public Task SmartDamageToTypes( this InvaderGroup grp, int startingDamage, params Invader[] invaderGeneric ) {
			int damageToInvaders = startingDamage;

			var allTargetTypes = invaderGeneric.SelectMany( x => x.AliveVariations ).ToArray();

			// While damage remains    &&    we have invaders
			IEnumerable<InvaderSpecific> Targets() => grp.InvaderTypesPresent_Specific.Intersect( allTargetTypes );

			while(damageToInvaders > 0 && Targets().Any()) {
				InvaderSpecific invaderToDamage = SmartInvaderAttacker.GetKillOrder( grp, damageToInvaders ).Where(i=>invaderGeneric.Contains(i.Generic)).First();
				damageToInvaders -= grp.ApplyDamageTo1( damageToInvaders, invaderToDamage );
			}

			return Task.CompletedTask;
		}

		class SmartInvaderAttacker {
			static public InvaderSpecific[] KillOrder = "C@1 C@2 C@3 T@1 T@2 E@1".Split( ' ' ).Select( k => InvaderSpecific.Lookup[k] ).ToArray();
			static public InvaderSpecific[] LeftOverOrder = "C@2 T@2 C@3".Split( ' ' ).Select( k => InvaderSpecific.Lookup[k] ).ToArray();

			static public IEnumerable<InvaderSpecific> GetKillOrder( InvaderGroup grp, int availableDamage ) {
				return KillOrder.Where( specific => specific.Health <= availableDamage )
					.Concat(LeftOverOrder)
					.Where( specific => grp[specific] > 0 );
			}

		}


	}

}