namespace SpiritIsland.JaggedEarth;

public class ElementalTeachings {

	public const string Name = "Elemental Teachings";

	[SpiritCard(ElementalTeachings.Name,0,Element.Moon,Element.Air,Element.Earth), Fast, AnySpirit]
	[Instructions( "Prepare 1 Element Marker. Discard up to 3 Element Markers. Target Spirit gains those Elements. (They can be any combination of Elements - the same or different.)" ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpiritCtx ctx ) {
		if(ctx.Self is not ShiftingMemoryOfAges smoa) return;

		// Prepare 1 Element Marker.
		await smoa.PreparedElementMgr.Prepare(ElementalTeachings.Name);

		// Discard up to 3 Element Markers.
		var discarded = await smoa.PreparedElementMgr.DiscardElements(3,"Target Spirit");

		// Target Spirit gains those Elements. (They can be any combination of elements)
		ctx.Other.Elements.Add( discarded );
	}

}