using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InstrumentsOfTheirOwnRuin {

		[MajorCard( "Instruments of Their Own Ruin", 4, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
		[Fast]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.SelectActionOption( 
				new ActionOption(
					"Add strife. Invaders with strife deal Damage to other Invaders in target land.", 
					() => AddStrifeThenStrifedInvadersDamageUnstrifed( ctx )
				)
				, new ActionOption(
					"Instead, if Invaders Ravage in target land, damage invaders in adjacent lands instead of dahan"
					, () => DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( ctx )
					, await ctx.YouHave("4 sun,2 fire,2 animal" )
				)
			);

		}

		public static async Task AddStrifeThenStrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
			// add 1 strife
			await ctx.AddStrife();

			await StrifedRavage.StrifedInvadersDamageUnstrifed( ctx );
		}


		static Task DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( TargetSpaceCtx ctx ) {
			// Note - this works regardless of them ravaging in target land or not. yay!
			int damageFromStrifedInvaders = ctx.Tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * ctx.Tokens[si] );
			async Task Sequence( RavageEngine eng ) {
				// they damage invaders in adjacent lands instead of dahan and the land.
				var invaderSpaceCtx = await ctx.SelectAdjacentLand( $"Apply {damageFromStrifedInvaders} damage to", x => x.HasInvaders );
				if(invaderSpaceCtx != null)
					await StrifedRavage.DamageUnStriffed( invaderSpaceCtx, damageFromStrifedInvaders );
				// dahan in target land do not fight back.
			}
			ctx.ModifyRavage( cfg => cfg.RavageSequence = Sequence );
			return Task.CompletedTask;
		}

	}

}
