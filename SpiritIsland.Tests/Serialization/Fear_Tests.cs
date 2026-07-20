namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for Fear.ToJson/FromJson and FearCardRegistry - docs/GameSerialization-Roadmap.md
/// section 6.
/// </summary>
public class Fear_Tests {

	[Fact]
	public void RoundTrips_CountersAndDeckActivatedCardOrder() {
		var gs = new SoloGameState();
		gs.Fear.PushOntoDeck( new SpiritIsland.Basegame.AvoidTheDahan() ); // now top of Deck
		gs.Fear.Add( gs.Fear.PoolMax ); // pops the pushed card into ActivatedCards

		JsonObject json = gs.Fear.ToJson();
		SpiritIsland.Fear restored = SpiritIsland.Fear.FromJson( json, gs );

		restored.EarnedFear.ShouldBe( gs.Fear.EarnedFear );
		restored.PoolMax.ShouldBe( gs.Fear.PoolMax );
		restored.ResolvedCardCount.ShouldBe( gs.Fear.ResolvedCardCount );
		restored.Deck.Select( c => c.GetType() ).ShouldBe( gs.Fear.Deck.Select( c => c.GetType() ) );
		restored.ActivatedCards.Select( c => c.GetType() ).ShouldBe( gs.Fear.ActivatedCards.Select( c => c.GetType() ) );
		restored.ActivatedCards.Peek().ShouldBeOfType<SpiritIsland.Basegame.AvoidTheDahan>();
	}

	[Fact]
	public async Task RoundTrips_FlippedState() {
		var gs = new SoloGameState();
		await using ActionScope scope = await ActionScope.Start( ActionCategory.Fear ); // Flipped=true logs via ActionScope.Current

		var card = new SpiritIsland.Basegame.AvoidTheDahan { Flipped = true };
		gs.Fear.PushOntoDeck( card );

		JsonObject json = gs.Fear.ToJson();
		SpiritIsland.Fear restored = SpiritIsland.Fear.FromJson( json, gs );

		restored.Deck.Peek().Flipped.ShouldBeTrue();
	}

	[Fact]
	public void FearCardRegistry_ResolvesByTypeName() {
		JsonArray json = new JsonArray( nameof( SpiritIsland.JaggedEarth.SenseOfDread ), false );

		IFearCard card = FearCardRegistry.Deserialize( json );

		card.ShouldBeOfType<SpiritIsland.JaggedEarth.SenseOfDread>();
		card.Flipped.ShouldBeFalse();
	}

}
