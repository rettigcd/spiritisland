using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ProwlingPanthers {

		[MinorCard( "Prowling Panthers", 1, Element.Moon, Element.Fire, Element.Animal ), Slow, FromPresence( 1, Target.JungleOrMountain )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new SpaceAction( "1 fear, add beast", FearAndBeast ),
				new SpaceAction( "destroy 1 explorer/town", DestroyExplorerTown, ctx.Beasts.Any )
			);
		}

		static void FearAndBeast( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Beasts.Add(1);
		}

		static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
			return ctx.Invaders.DestroyAny( 1, Invader.Explorer, Invader.Town );
		}

	}

}
