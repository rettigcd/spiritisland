using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InstrumentsOfTheirOwnRuin {

		[MajorCard( "Instruments of Their Own Ruin", 4, Speed.Fast, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 strife
			await ctx.AddStrife();

			// See default action below

			// if you have 4 sun, 2 fire 2 animal:
			bool invadersRavageInTargetLand = false; // !!! Ravage status must be known at start of round in order to detect this.
			if(invadersRavageInTargetLand && ctx.YouHave("4 sun,2 fire,2 animal" )) {
				// Instead, if invaders ravage in target land, they damage invaders in adjacent lands instead of dahan and the land.
				// dahan in target land do not fight back.
				// !!! configure Ravage to deal damage to neightbors
			} else {

				// This is the default action

				// each invader with strife deals damage to other invaders in target land
				int damageFromStrifedInvaders = ctx.Tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * ctx.Tokens[si] );
				await ctx.DamageInvaders( damageFromStrifedInvaders ); // !! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
			}

		}

	}

}
