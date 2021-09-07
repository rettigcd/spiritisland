using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GrantHatredARavenousForm {

		[MajorCard( "Grant Hatred a Ravenous Form", 4, Speed.Slow, Element.Moon, Element.Fire )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// for each strife or blight in target land, 1 fear and 2 damage.
			// if this destorys all invaders in target land, add 1 beast.

			// if you have 4 moon, 2 fire: add 1 strife in up to 3 adjacent lands.
			return Task.CompletedTask;
		}

	}

}
