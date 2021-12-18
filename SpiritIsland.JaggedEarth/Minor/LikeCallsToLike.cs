using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class LikeCallsToLike{ 

		[MinorCard("Like Calls to Like",1,Element.Sun,Element.Water,Element.Plant),Slow,FromPresence(1)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// If target land has explorer, Gather up to 1 explorer.
			await GatherLike( ctx, Invader.Explorer );
			// Do likewise for town, dahan, blight, and beast.
			await GatherLike( ctx, Invader.Town );
			await GatherLike( ctx, TokenType.Dahan );
			await GatherLike( ctx, TokenType.Blight );
			await GatherLike( ctx, TokenType.Beast );
		}

		static async Task GatherLike( TargetSpaceCtx ctx, TokenCategory tokenTypeOfInterest ) {
			if(ctx.Tokens.HasAny( tokenTypeOfInterest ))
				await ctx.GatherUpTo( 1, tokenTypeOfInterest );
		}

	}

}
