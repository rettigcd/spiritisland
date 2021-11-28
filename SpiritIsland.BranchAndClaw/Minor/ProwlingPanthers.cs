using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ProwlingPanthers {

		[MinorCard( "Prowling Panthers", 1, Element.Moon, Element.Fire, Element.Animal ), Slow, FromPresence( 1, Target.JungleOrMountain )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption( "1 fear, add beast", ()=>FearAndBeast(ctx) ),
				new ActionOption( "destroy 1 explorer/town", ()=>DestroyExplorerTown(ctx), ctx.Beasts.Any )
			);
		}

		static void FearAndBeast( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Beasts.Count++;
		}

		static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
			return ctx.Invaders.DestroyAny( 1, Invader.Explorer, Invader.Town );
		}

	}

}
