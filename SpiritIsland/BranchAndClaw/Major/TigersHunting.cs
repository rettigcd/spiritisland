using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TigersHunting {

        [MajorCard("Tigers Hunting", 2, Speed.Fast, Element.Sun, Element.Moon, Element.Animal)]
        [FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
        static public Task ActAsync(TargetSpaceCtx ctx) {
			// 2 fear
			// add 1 beast. Gather up to 1 beast.
			// 1 damage per beast.  Push up to 2 beast
			// if you have 2 sun 2 moon 3 animal
			//   1 damage in adjacent land without blight, and +1 damage per beast there
            return Task.CompletedTask;
        }

    }

}
