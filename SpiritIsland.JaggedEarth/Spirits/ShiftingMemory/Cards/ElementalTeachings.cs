namespace SpiritIsland.JaggedEarth;

public class ElementalTeachings {

	public const string Name = "Elemental Teachings";

	[SpiritCard(ElementalTeachings.Name,0,Element.Moon,Element.Air,Element.Earth), Fast, AnySpirit]
	static public async Task ActAsync(TargetSpiritCtx ctx ) {
		if(ctx.Self is not ShiftingMemoryOfAges smoa) return;

		// Prepare 1 Element Marker.
		await smoa.PrepareElement(ElementalTeachings.Name);

		// Discard up to 3 Element Markers.
		var discarded = await smoa.DiscardElements(3,"Target Spirit");

		// Target Spirit gains those Elements. (They can be any combination of elements)
		foreach(var el in discarded)
			ctx.Other.Elements[el.Key] += el.Value;
	}

}