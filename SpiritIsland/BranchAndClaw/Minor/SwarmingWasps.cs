using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class SwarmingWasps {

		[MinorCard( "Swarming Wasps", 0, Speed.Fast, Element.Fire, Element.Air, Element.Animal )]
		[FromPresence( 1, Target.NoBlight )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( "Add 1 beast", ctx=>ctx.Tokens[BacTokens.Beast]++),
				new PowerOption( "Push up to 2 explorers", ctx => ctx.PushUpToNTokens( 2, Invader.Explorer ), ctx.Tokens.Has(BacTokens.Beast) )
			);

		}

	}
}
