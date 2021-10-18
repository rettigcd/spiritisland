﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public static class StrifedRavage {

		public static async Task StrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
			// Each invader with strife deals damage to other invaders in target land
			int damageFromStrifedInvaders = DamageFromStrifedInvaders( ctx.Tokens );
			await DamageUnStriffed( ctx, damageFromStrifedInvaders );
		}

		public static int DamageFromStrifedInvaders( TokenCountDictionary tokens ) {
			return tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * tokens[si] );
		}

		public static int DamageFrom1StrifedInvaders( TokenCountDictionary tokens ) {
			var strifedInvaderWithMostDamage = tokens.Invaders().OfType<StrifedInvader>()
				.OrderByDescending(x=>x.FullHealth)
				.FirstOrDefault();
			return strifedInvaderWithMostDamage != null ? strifedInvaderWithMostDamage.FullHealth : 0;
		}


		static public async Task DamageUnStriffed( TargetSpaceCtx invaderSpaceCtx, int damageFromStrifedInvaders ) {
			// !!! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
			// ! Fix by passing in a predicate
			await invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
		}

		public static void StrifedInvadersLoseHealthPerStrife( FearCtx ctx ) {
			// !!! We need a reset other than end-of-round when Silent Shroud is in play
			foreach(var space in ctx.GameState.Island.AllSpaces) {
				var tokens = ctx.InvadersOn( space ).Tokens;
				var strifedInvaders = tokens.Invaders()
					.OfType<StrifedInvader>()
					.Where( x => x.Health > 1 )
					.OrderBy( x => x.Health ); // get the lowest ones first so we can reduce without them cascading
				foreach(StrifedInvader strifedInvader in strifedInvaders) {
					var newInvader = strifedInvader.ResultingDamagedInvader( strifedInvader.StrifeCount );
					if(newInvader.Health > 0) {
						tokens[newInvader] = tokens[strifedInvader];
						tokens[strifedInvader] = 0;
					}
				}
			}
		}


	}

}
