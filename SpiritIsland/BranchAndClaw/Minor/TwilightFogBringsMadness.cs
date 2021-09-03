using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TwilightFogBringsMadness {

		[MinorCard( "Twilight Fog Brings Madness", 0, Speed.Slow, Element.Sun, Element.Moon, Element.Air, Element.Water )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			
			await ctx.Self.SelectInvader_ToStrife(ctx.Tokens);
			await ctx.PushUpToNTokens(1,TokenType.Dahan);
			
			// !!! destroy dahan with 1 damage first!
			ctx.Tokens[TokenType.Dahan[1]] = ctx.Tokens[TokenType.Dahan[2]];
			ctx.Tokens[TokenType.Dahan[2]] = 0;
			// !!! also, these dahan have no way to heal...

		}


	}

}
