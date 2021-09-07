using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InsatiableHungerOfTheSwarm {

		[MajorCard( "Insatiable Hunger of the Swarm", 4, Speed.Fast, Element.Air, Element.Plant, Element.Animal )]
		[FromSacredSite( 2 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 blight.  Add 2 beasts, gather up to 2 beasts
			// each beast deal 1 fear, 2 damage to invaders and 2 damage to dahan.  Destroy 1 beast.
			// if you have 2 air, 4 animal, repeat power on adjacent land.
			return Task.CompletedTask;
		}


	}

}
