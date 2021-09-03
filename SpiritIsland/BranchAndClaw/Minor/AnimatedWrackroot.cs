using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AnimatedWrackroot {

		[MinorCard( "Animated WrackRoot", 0, Speed.Slow, Element.Moon, Element.Fire, Element.Plant )]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectPowerOption(
				new PowerOption( "1 fear, Destory 1 explorer", FearAndExplorer ),
				new PowerOption("add 1 wilds", ctx => ctx.Tokens[BacTokens.Wilds]++ )
			);
		}

		private static async Task FearAndExplorer( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			await ctx.InvadersOn( ctx.Target ).Destroy( 1, Invader.Explorer[1] );
		}
	}


}
