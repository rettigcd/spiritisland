using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {

	public class ElementalTeachings {

		[SpiritCard("Elemental Teachings",0,Element.Moon,Element.Air,Element.Earth), Fast, AnySpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) {
			// Prepare 1 Element Marker.
			await ctx.Self.PrepareElement();

			// Discard up to 3 Element Markers.
			var discarded = await ctx.Self.DiscardElements(3);

			// Target Spirit gains those Elements. (They can be any combination of elements)
			foreach(var el in discarded)
				ctx.Other.Elements[el.Key] += el.Value;
		}

	}

}
