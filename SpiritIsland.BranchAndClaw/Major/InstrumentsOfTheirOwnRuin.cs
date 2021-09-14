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
			// Instead, if invaders ravage in target land,
			bool invadersRavageInTargetLand = ctx.GameState.ScheduledRavageSpaces.Contains(ctx.Space);
			if(invadersRavageInTargetLand && ctx.YouHave("4 sun,2 fire,2 animal" )) {
				int damageFromStrifedInvaders = ctx.Tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * ctx.Tokens[si] );
				async Task Sequence(RavageEngine eng ) {
					// they damage invaders in adjacent lands instead of dahan and the land.
					var invaderSpaceCtx = await ctx.SelectAdjacentLand($"Apply {damageFromStrifedInvaders} damage to", x=>x.HasInvaders);
					if(invaderSpaceCtx != null)
						await DamageUnStriffed( invaderSpaceCtx, damageFromStrifedInvaders );
					// dahan in target land do not fight back.
				}
				ctx.ModifyRavage(cfg => cfg.RavageSequence = Sequence );
			} else {
				// This is the default action

				// each invader with strife deals damage to other invaders in target land
				await StrifedInvadersDamageUnstrifed( ctx );
			}

		}

		static async Task StrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
			int damageFromStrifedInvaders = DamageFromStrifedInvaders( ctx.Tokens );
			await DamageUnStriffed( ctx, damageFromStrifedInvaders );
		}

		static public int DamageFromStrifedInvaders( TokenCountDictionary tokens ) { // Similar to Discord
			return tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * tokens[si] );
		}

		static public async Task DamageUnStriffed( TargetSpaceCtx invaderSpaceCtx, int damageFromStrifedInvaders ) {
			// !! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
			await invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
		}

	}

}
