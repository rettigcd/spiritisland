using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SapTheStrengthOfMultitudes {

		[MinorCard( "Sap the Strength of Multitudes", 0, "water, animal" )]
		[Fast]
		[ExtendableRange(From.Presence,0,Target.Any,"1 air",1)]
		static public Task ActAsync( TargetSpaceCtx ctx) {
			// defend 5
			ctx.Defend(5);
			return Task.CompletedTask;
		}

	}

}
