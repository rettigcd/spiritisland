using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SeaMonsters {

		[MajorCard( "Sea Monsters", 5, Speed.Slow, Element.Water, Element.Animal )]
		[FromPresence( 1, Target.CoastalOrWetlands )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 beast.  IF invaders are present, 2 fear per beast (max 8 fear).  3 damage per besast, 1 damage per blight
			// if you have 3 water and 3 animal: repeat this power
			return Task.CompletedTask;
		}

	}


}
