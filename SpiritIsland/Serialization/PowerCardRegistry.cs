namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves a PowerCard by Title alone - a single registry shared by Major cards, Minor cards, and
/// every spirit's own Spirit-type starting cards. PowerCard is fully immutable (every field readonly,
/// built once via ForDecorated), so unlike BlightCard/InvaderCard/IFearCard there's no per-instance
/// state that needs a fresh object per deserialize - the same registered instance is reused everywhere
/// it's referenced, the same way ITokenClass/SpaceSpec singletons are reused. Every card's Title is
/// unique across the whole pool by design - `Register` below throws if two different cards ever
/// collide on the same title - so nothing about the JSON needs to say which pool a card came from.
///
/// Seeded from each expansion's existing GameComponentProvider.MinorCards/MajorCards/SpiritNames (the
/// same lists real game setup already uses to build decks and spirits) via that provider's own
/// [ModuleInitializer] - no per-card registration needed.
/// </summary>
public static class PowerCardRegistry {

	public static PowerCard Deserialize( JsonNode json ) => _cards[json.GetValue<string>()];

	public static void Register( PowerCard card ) {
		// Aspects/multiple spirits can legitimately re-register the exact same underlying card (e.g. an
		// aspect that doesn't touch a given starting card still re-exposes it unchanged) - only a
		// genuine collision (same title, different card) is an error.
		if( _cards.TryGetValue( card.Title, out PowerCard? existing )
			&& ( existing.Cost != card.Cost || existing.Instructions != card.Instructions || existing.PowerType != card.PowerType ) )
			throw new InvalidOperationException( $"PowerCard collision: '{card.Title}' resolves to two different cards" );
		_cards[card.Title] = card;
	}

	static readonly Dictionary<string, PowerCard> _cards = [];

}
