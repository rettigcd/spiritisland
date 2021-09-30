using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PreyOnTheBuilders {

		[SpiritCard( "Prey on the Builders", 1, Speed.Fast, Element.Moon, Element.Fire, Element.Animal )]
		[FromPresence(0)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// you may gather 1 beast
			await ctx.GatherUpTo(1, BacTokens.Beast.Generic);

			if( ctx.Tokens.Beasts().Any )
				ctx.GameState.Skip1Build(ctx.Space);

		}

	}

}
