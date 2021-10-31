using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class ElementalTeachings {

		[SpiritCard("Elemental Teachings",0,Element.Moon,Element.Air,Element.Earth), Fast, AnySpirit]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// Prepare 1 Element Marker.
			// Discard up to 3 Element Markers.
			// Target Spirit gains those Elements. (They can be any combination of elements)
			return Task.CompletedTask;
		}

	}

}
