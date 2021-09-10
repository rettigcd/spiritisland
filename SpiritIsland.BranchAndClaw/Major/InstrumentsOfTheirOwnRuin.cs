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

			int damageFromStrifedInvaders = ctx.Tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * ctx.Tokens[si] );


			// if you have 4 sun, 2 fire 2 animal:
			// Instead, if invaders ravage in target land,
			bool invadersRavageInTargetLand = ctx.GameState.ScheduledRavageSpaces.Contains(ctx.Space);
			if(invadersRavageInTargetLand && ctx.YouHave("4 sun,2 fire,2 animal" )) {
				async Task Sequence(RavageEngine eng ) {
					// they damage invaders in adjacent lands instead of dahan and the land.
					var invaderSpaceCtx = await ctx.SelectAdjacentLand($"Apply {damageFromStrifedInvaders} damage to", x=>x.HasInvaders);
					if(invaderSpaceCtx != null)
						await DamageUnStriffed( damageFromStrifedInvaders, invaderSpaceCtx );
					// dahan in target land do not fight back.
				}
				ctx.ModifyRavage(cfg => cfg.RavageSequence = Sequence );
			} else {

				// This is the default action

				// each invader with strife deals damage to other invaders in target land
				await DamageUnStriffed( damageFromStrifedInvaders, ctx );
			}

		}

		static async Task DamageUnStriffed( int damageFromStrifedInvaders, TargetSpaceCtx invaderSpaceCtx ) {
			// !! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
			await invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
		}
	}

}
