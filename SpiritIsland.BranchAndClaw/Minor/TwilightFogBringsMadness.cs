using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TwilightFogBringsMadness {

		[MinorCard( "Twilight Fog Brings Madness", 0, Speed.Slow, Element.Sun, Element.Moon, Element.Air, Element.Water )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// Add 1 strife
			await ctx.Self.SelectInvader_ToStrife( ctx.Tokens );
			// Push 1 dahan
			await ctx.PushUpToNTokens( 1, TokenType.Dahan );

			// Each remaining Dahan take 1 damage
			await RemainingDahanTake1Damage( ctx );

		}

		static async Task RemainingDahanTake1Damage( TargetSpaceCtx ctx ) {
			int dahanDestroyed = ctx.Tokens[TokenType.Dahan[1]];
			ctx.Tokens[TokenType.Dahan[1]] = ctx.Tokens[TokenType.Dahan[2]];
			ctx.Tokens[TokenType.Dahan[2]] = 0;
			await ctx.GameState.Tokens.TokenDestroyed.InvokeAsync( ctx.GameState, new TokenDestroyedArgs {
				Token = TokenType.Dahan,
				space = ctx.Target,
				count = dahanDestroyed,
				Source = Cause.Power
			} );
		}
	}

}
