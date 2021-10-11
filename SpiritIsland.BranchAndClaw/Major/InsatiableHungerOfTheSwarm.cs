using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InsatiableHungerOfTheSwarm {

		[MajorCard( "Insatiable Hunger of the Swarm", 4, Element.Air, Element.Plant, Element.Animal )]
		[Fast]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			static async Task ApplyPowerOnTarget( TargetSpaceCtx ctx ) {
				// add 1 blight.
				ctx.AddBlight( 1 );

				// Add 2 beasts
				var beasts = ctx.Tokens.Beasts();
				beasts.Count += 2;

				// Gather up to 2 beasts
				await ctx.GatherUpTo( 2, BacTokens.Beast.Generic );

				// each beast deals:
				// 1 fear
				ctx.AddFear( beasts.Count );
				// 2 damage to invaders
				await ctx.DamageInvaders( beasts.Count * 2 );
				// and 2 damage to dahan.
				await ApplyDamageToDahan( ctx, beasts.Count );

				// Destroy 1 beast.
				beasts.Count--;
			}
			await ApplyPowerOnTarget( ctx );

			// if you have 2 air, 4 animal, repeat power on adjacent land.
			if(ctx.YouHave( "2 air,4 animal" )) {
				var adjCtx = await ctx.SelectAdjacentLand("Apply power to adjacent land");
				await ApplyPowerOnTarget( adjCtx );
			}
		}

		static async Task ApplyDamageToDahan( TargetSpaceCtx ctx, int damageToDahan ) {
			while(damageToDahan > 0 && ctx.HasDahan) {
				if(ctx.Tokens[TokenType.Dahan[1]] > 0) {
					await ctx.DestroyDahan( 1, TokenType.Dahan[1] );
					damageToDahan--;
				} else if(damageToDahan > 1) {
					await ctx.DestroyDahan( 1, TokenType.Dahan[2] );
					damageToDahan -= 2;
				} else {
					ctx.Tokens[TokenType.Dahan[1]]++;
					ctx.Tokens[TokenType.Dahan[2]]--;
				}
			}

		}
	}

}
