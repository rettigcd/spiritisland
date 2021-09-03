using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ProwlingPanthers {

		[MinorCard( "Prowling Panthers", 1, Speed.Slow, Element.Moon, Element.Fire, Element.Animal )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectPowerOption(
				new PowerOption( "1 fear, add beast", FearAndBeast ),
				new PowerOption( "destroy 1 explorer/town", DestroyExplorerTown, ctx.Tokens.Has(BacTokens.Beast) )
			);
		}

		static void FearAndBeast( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Tokens[BacTokens.Beast]++;
		}

		static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
			return ctx.InvadersOn( ctx.Target ).DestroyAny( 1, Invader.Explorer, Invader.Town );
		}

	}

}
