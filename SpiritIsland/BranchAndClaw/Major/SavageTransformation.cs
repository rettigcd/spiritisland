using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SavageTransformation {

		[MajorCard( "Savage Transformation", 2, Speed.Slow, Element.Moon, Element.Animal )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// 2 fear
			// replace 1 explorer with 1 beast
			// if you have 2 moon, 3 animal: replace 1 additional explorer with 1 beat in either target or adjacent land
			return Task.CompletedTask;
		}

	}
}
