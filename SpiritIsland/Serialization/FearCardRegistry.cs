namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an IFearCard by its type name. Same shape as BlightCardRegistry/SpaceEntitySerialization,
/// kept separate because IFearCard isn't ISpaceEntity - it's held in Fear's Deck/ActivatedCards, not
/// placed on a Space. No ISerializationContext needed in the reader signature (unlike
/// BlightCardRegistry) - unlike BlightCard, every one of the ~50 IFearCard implementers was checked
/// (no readonly/mutable instance fields beyond what FearCardBase already provides) and none references
/// a Spirit or other context-resolved value, so there's nothing for a reader to need it for. If a
/// future fear card needs extra state, widen this the same way BlightCardRegistry's reader signature
/// was widened for SlowDissolutionOfWill - that was a small, mechanical change, not a rewrite.
/// </summary>
public static class FearCardRegistry {

	public static IFearCard Deserialize( JsonArray json ) {
		string name = json[0]!.GetValue<string>();
		IFearCard card = _readers[name]();
		card.Flipped = json[1]!.GetValue<bool>();
		return card;
	}

	public static void Register( string name, Func<IFearCard> factory ) => _readers[name] = factory;

	static readonly Dictionary<string, Func<IFearCard>> _readers = [];

}
