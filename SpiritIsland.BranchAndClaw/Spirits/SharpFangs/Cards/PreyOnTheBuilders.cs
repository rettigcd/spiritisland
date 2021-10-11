using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PreyOnTheBuilders {

		[SpiritCard( "Prey on the Builders", 1, Element.Moon, Element.Fire, Element.Animal )]
		[Fast]
		[FromPresence(0)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// you may gather 1 beast
			await ctx.GatherUpTo(1, BacTokens.Beast.Generic);

			if( ctx.Tokens.Beasts().Any )
				ctx.Skip1Build();

		}

	}

}
