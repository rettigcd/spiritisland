namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an InvaderCard by its Code (e.g. "J", "2Coastal", "J+S") - InvaderCard is immutable apart
/// from Flipped, and every standard card's identity is already fully described by (Filter, InvaderStage,
/// TriggersEscalation), which InvaderDeckBuilder.Level1Cards/Level2Cards/Level3Cards already know how to
/// build. Rather than hand-writing a factory per card, InvaderDeckBuilder's own [ModuleInitializer]
/// (see InvaderDeckBuilder.cs) seeds this registry by iterating those three lists once - reusing the
/// existing "build all cards" engine instead of duplicating it.
///
/// A handful of adversaries define their own one-off cards outside the standard terrain set (e.g.
/// HabsburgMiningExpedition's "Salt Deposits") - those register themselves here the same way, from
/// their own [ModuleInitializer].
/// </summary>
public static class InvaderCardRegistry {

	public static InvaderCard Deserialize( JsonArray json ) {
		string code = json[0]!.GetValue<string>();
		InvaderCard card = _factories[code]();
		card.Flipped = json[1]!.GetValue<bool>();
		return card;
	}

	public static void Register( string code, Func<InvaderCard> factory ) => _factories[code] = factory;

	static readonly Dictionary<string, Func<InvaderCard>> _factories = [];

}
