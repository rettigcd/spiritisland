using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InstrumentsOfTheirOwnRuin {

		[MajorCard( "Instruments of Their Own Ruin", 4, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
		[Fast]
		[FromSacredSite( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption( 
				new ActionOption(
					"Add strife. Invaders with strife deal Damage to other Invaders in target land.", 
					() => StrifedInvadersDamageUnstrifed( ctx )
				)
				, new ActionOption(
					"Instaed, if Invaders Ravage in target land, damage invaders in adjacent lands instead of dahan"
					, () => DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( ctx )
					,ctx.YouHave("4 sun,2 fire,2 animal" )
				)
			);

		}

		static async Task StrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
			// add 1 strife
			await ctx.AddStrife();
			// Each invader with strife deals damage to other invaders in target land
			int damageFromStrifedInvaders = DamageFromStrifedInvaders( ctx.Tokens );
			await DamageUnStriffed( ctx, damageFromStrifedInvaders );
		}

		static Task DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( TargetSpaceCtx ctx ) {
			// Note - this works regardless of them ravaging in target land or not. yay!
			int damageFromStrifedInvaders = ctx.Tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * ctx.Tokens[si] );
			async Task Sequence( RavageEngine eng ) {
				// they damage invaders in adjacent lands instead of dahan and the land.
				var invaderSpaceCtx = await ctx.SelectAdjacentLand( $"Apply {damageFromStrifedInvaders} damage to", x => x.HasInvaders );
				if(invaderSpaceCtx != null)
					await DamageUnStriffed( invaderSpaceCtx, damageFromStrifedInvaders );
				// dahan in target land do not fight back.
			}
			ctx.ModifyRavage( cfg => cfg.RavageSequence = Sequence );
			return Task.CompletedTask;
		}

		static public int DamageFromStrifedInvaders( TokenCountDictionary tokens ) { // Similar to Discord
			return tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * tokens[si] );
		}

		static public async Task DamageUnStriffed( TargetSpaceCtx invaderSpaceCtx, int damageFromStrifedInvaders ) {
			// !!! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
			await invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
		}

	}

}
