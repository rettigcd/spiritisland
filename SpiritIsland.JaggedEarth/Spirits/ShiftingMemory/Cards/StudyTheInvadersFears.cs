using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class StudyTheInvadersFears {
		[SpiritCard("Study the Invaders' Fears",0,Element.Moon,Element.Air,Element.Animal),FromPresence(0,Target.TownOrExplorer)]
		static public Task ActAsync(TargetSpaceCtx ctx ) { 
			// 2 fear.
			// Turn the top card of the Fear Deck face-up.
			return Task.CompletedTask;
		}

	}

}
