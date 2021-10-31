using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class BoonOfAncientMemories {

		[SpiritCard("Boon of Ancient Memories",1,Element.Moon,Element.Water,Element.Earth,Element.Plant), Slow, AnySpirit]
		static public Task ActAsync(TargetSpaceCtx ctx ) { 
			// if you target yourself, gain a Minor Power.
			// Otherwise: Target Spirit gains a Power Card.  If it's a Major Power, they may pay 2 Energy instead of Forgetting a Power Card.
			return Task.CompletedTask;
		}

	}

}
