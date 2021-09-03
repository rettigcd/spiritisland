using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CyclesOfTimeAndTide {

		[MinorCard( "Cycles of Time and Tide", 1, Speed.Fast, Element.Sun, Element.Moon, Element.Water )]
		[FromPresence( 1, Target.Costal )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			if(ctx.HasDahan)
				ctx.Tokens[TokenType.Dahan.Default]++;
			else if( ctx.Tokens.Has(TokenType.Blight) )
				ctx.Tokens[TokenType.Blight]--;

		}

	}

}
