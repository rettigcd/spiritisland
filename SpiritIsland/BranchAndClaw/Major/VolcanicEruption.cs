using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class VolcanicEruption {
        [MajorCard("Volcanic Eruption", 8, Speed.Slow, Element.Fire, Element.Earth)]
        [FromPresenceIn(1,Terrain.Mountain)]
        static public Task ActAsync(TargetSpaceCtx ctx) {
			// 6 fear
			// 20 damage. Destroy all dahan and beast.  Add 1 blight
			// if you have 4 fire, 3 earth:  Destory all invaders.  Add 1 wilds.  In  each adjacent land: 10 damage, destory all dahan and beast.  IF there are no blight, add 1 blight
            return Task.CompletedTask;
        }
    }

}
