using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AnimatedWrackroot {

		[MinorCard( "Animated WrackRoot", 0, Speed.Slow, Element.Moon, Element.Fire, Element.Plant )]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption( "1 fear, Destory 1 explorer", ()=>FearAndExplorer(ctx) ),
				new ActionOption("add 1 wilds", () => ctx.Tokens.Wilds().Count++ )
			);
		}

		private static async Task FearAndExplorer( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			await ctx.PowerInvaders.Destroy( 1, Invader.Explorer[1] );
		}
	}


}
