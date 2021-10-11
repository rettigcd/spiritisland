using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CyclesOfTimeAndTide {

		[MinorCard( "Cycles of Time and Tide", 1, Element.Sun, Element.Moon, Element.Water )]
		[Fast]
		[FromPresence( 1, Target.Coastal )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			if(ctx.HasDahan)
				ctx.Tokens[TokenType.Dahan.Default]++;
			else
				ctx.RemoveBlight();

			return Task.CompletedTask;
		}

	}

}
