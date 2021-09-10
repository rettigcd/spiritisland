using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InsatiableHungerOfTheSwarm {

		[MajorCard( "Insatiable Hunger of the Swarm", 4, Speed.Fast, Element.Air, Element.Plant, Element.Animal )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			await ApplyPowerOnTarget( ctx );

			// if you have 2 air, 4 animal, repeat power on adjacent land.
			if(ctx.Self.Elements.Contains( "2 air,4 animal" )) {
				var otherSpace = await ctx.Self.Action.Decide( new SelectAdjacentDecision( "Repeate Power", ctx.Target, GatherPush.None, ctx.PowerAdjacents() ) );
				await ApplyPowerOnTarget( new TargetSpaceCtx(ctx.Self,ctx.GameState,otherSpace) );
			}
		}

		static async Task ApplyPowerOnTarget( TargetSpaceCtx ctx ) {
			// add 1 blight.
			ctx.AddBlight( 1 );

			// Add 2 beasts
			var beasts = ctx.Tokens.Beasts();
			beasts.Count += 2;

			// Gather up to 2 beasts
			ctx.GatherUpToNTokens( 2, BacTokens.Beast.Generic );

			// each beast deals:
			// 1 fear
			ctx.AddFear( beasts.Count );
			// 2 damage to invaders
			await ctx.PowerInvaders.ApplySmartDamageToGroup( beasts.Count * 2 );
			// and 2 damage to dahan.
			await ctx.GameState.DahanDestroy( ctx.Target, beasts.Count, Cause.Power ); // !!! this is not correct, dahan might have 1 health

			// Destroy 1 beast.
			beasts.Count--;
		}
	}

}
