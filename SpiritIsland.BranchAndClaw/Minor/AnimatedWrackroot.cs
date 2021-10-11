using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AnimatedWrackroot {

		[MinorCard( "Animated WrackRoot", 0, Element.Moon, Element.Fire, Element.Plant )]
		[Slow]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption( "1 fear, Destory 1 explorer", ()=>FearAndExplorer(ctx) ),
				new ActionOption("add 1 wilds", () => ctx.Tokens.Wilds().Count++ )
			);
		}

		private static async Task FearAndExplorer( TargetSpaceCtx ctx ) {
			// 1 fear
			ctx.AddFear( 1 );
			// destory 1 explorer
			await ctx.Invaders.Destroy( 1, Invader.Explorer[1] );
		}
	}


}
