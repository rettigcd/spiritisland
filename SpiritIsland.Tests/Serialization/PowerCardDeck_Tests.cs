namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for PowerCardDeck.ToJson/RestoreFromJson - docs/GameSerialization-Roadmap.md's
/// PowerCardDeck section. `_randomizer`'s seed is deliberately not covered here - a separate,
/// still-open policy decision (the Determinism/RNG section).
/// </summary>
public class PowerCardDeck_Tests {

	[Fact]
	public void RoundTrips_CardsAndDiscards_PreservingStackOrder() {
		var deck = new PowerCardDeck( new PowerCard[] {
			PowerCard.ForDecorated( Drought.ActAsync ),
			PowerCard.ForDecorated( DevouringAnts.ActAsync ),
			PowerCard.ForDecorated( DriftDownIntoSlumber.ActAsync ),
			PowerCard.ForDecorated( EnticingSplendor.ActAsync ),
			PowerCard.ForDecorated( LureOfTheUnknown.ActAsync ),
		}, seed: 12345, PowerType.Minor );

		// Flip some into "discards" to exercise both stacks, leaving the rest shuffled in _cards.
		List<PowerCard> discarded = deck.Flip( 2 );
		deck.Discard( discarded );

		JsonObject json = deck.ToJson();

		// Captured *after* serializing, so exercising the original deck doesn't change what was saved -
		// this is "what the next 3 draws would have been" for the state the JSON snapshot holds.
		string originalCardOrder = deck.Flip( 3 ).Select( c => c.Title ).Join( "," );

		var restored = new PowerCardDeck( new PowerCard[] {
			PowerCard.ForDecorated( Drought.ActAsync ),
			PowerCard.ForDecorated( DevouringAnts.ActAsync ),
			PowerCard.ForDecorated( DriftDownIntoSlumber.ActAsync ),
			PowerCard.ForDecorated( EnticingSplendor.ActAsync ),
			PowerCard.ForDecorated( LureOfTheUnknown.ActAsync ),
		}, seed: 999, PowerType.Minor ); // different seed/shuffle - RestoreFromJson must override it
		restored.RestoreFromJson( json );

		// Flipping the restored deck reproduces the exact same draw order the original had left.
		restored.Flip( 3 ).Select( c => c.Title ).Join( "," ).ShouldBe( originalCardOrder );
	}

	[Fact]
	public void RoundTrips_EmptyCardsDeck_TriggeringReshuffleFromDiscards() {
		var deck = new PowerCardDeck( new PowerCard[] {
			PowerCard.ForDecorated( Drought.ActAsync ),
			PowerCard.ForDecorated( DevouringAnts.ActAsync ),
		}, seed: 1, PowerType.Minor );

		List<PowerCard> allCards = deck.Flip( 2 ); // _cards now empty
		deck.Discard( allCards );

		JsonObject json = deck.ToJson();

		var restored = new PowerCardDeck( new PowerCard[] {
			PowerCard.ForDecorated( Drought.ActAsync ),
			PowerCard.ForDecorated( DevouringAnts.ActAsync ),
		}, seed: 2, PowerType.Minor );
		restored.RestoreFromJson( json );

		// _cards is empty, _discards holds both - flipping forces a reshuffle-from-discards, same as
		// it would on the original deck, without throwing.
		restored.Flip( 2 ).Select( c => c.Title ).OrderBy( t => t ).ShouldBe( allCards.Select( c => c.Title ).OrderBy( t => t ) );
	}

}
