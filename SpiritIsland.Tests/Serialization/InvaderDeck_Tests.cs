namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for InvaderCard/InvaderSlot/InvaderDeck - docs/GameSerialization-Roadmap.md section 5.
/// </summary>
public class InvaderDeck_Tests {

	[Fact]
	public void InvaderCard_RoundTrips_IdentityAndFlipped() {
		InvaderCard original = InvaderCard.Stage1( Terrain.Jungle );
		original.Flipped = true;

		JsonArray json = original.ToJson();
		InvaderCard restored = InvaderCardRegistry.Deserialize( json );

		restored.Code.ShouldBe( original.Code );
		restored.InvaderStage.ShouldBe( original.InvaderStage );
		restored.Flipped.ShouldBeTrue();
	}

	[Fact]
	public void InvaderCardRegistry_ResolvesEveryStandardCard() {
		// InvaderDeckBuilder's [ModuleInitializer] seeds the registry from these exact lists - prove
		// every one of them actually resolves back by Code, not just a hand-picked sample.
		foreach( InvaderCard card in InvaderDeckBuilder.Level1Cards.Concat( InvaderDeckBuilder.Level2Cards ).Concat( InvaderDeckBuilder.Level3Cards ) ) {
			InvaderCard restored = InvaderCardRegistry.Deserialize( card.ToJson() );
			restored.Code.ShouldBe( card.Code );
			restored.InvaderStage.ShouldBe( card.InvaderStage );
		}
	}

	[Fact]
	public void InvaderCardRegistry_ResolvesSaltDeposits() {
		// The one adversary-specific card outside the standard terrain set (HabsburgMiningExpedition) -
		// registers itself the same way the standard cards do, just not via InvaderDeckBuilder's list.
		InvaderCard original = SpiritIsland.NatureIncarnate.HabsburgMiningExpedition.SaltDepositDeckBuilder.SaltDeposits();

		InvaderCard restored = InvaderCardRegistry.Deserialize( original.ToJson() );

		restored.Code.ShouldBe( "Salt Deposits" );
	}

	[Fact]
	public void InvaderSlot_RoundTrips_CardsAndCounts() {
		var slot = new ExploreSlot();
		slot.Cards.Add( InvaderCard.Stage1( Terrain.Jungle ) );
		slot.Cards.Add( InvaderCard.Stage1( Terrain.Wetland ) );
		slot.HoldNextBack();
		slot.SkipNextNormal();

		JsonArray json = slot.ToJson();
		var restored = new ExploreSlot();
		restored.RestoreFromJson( json );

		restored.Cards.Select( c => c.Code ).ShouldBe( slot.Cards.Select( c => c.Code ) );

		// _holdBackCount survived: the first card advanced is held back, leaving only the second.
		List<InvaderCard> advanced = restored.GetCardsToAdvance();
		advanced.Select( c => c.Code ).ShouldBe( ["W"] );
	}

	[Fact]
	public async Task InvaderDeck_RoundTrips_FullState() {
		InvaderDeck original = InvaderDeckBuilder.Default.Build( 12345 );
		await original.InitExploreSlotAsync();

		// Move a card into Build to exercise more than one slot, and add a Discard.
		InvaderCard movedCard = original.Explore.Cards[0];
		original.Explore.Cards.Remove( movedCard );
		original.Build.Cards.Add( movedCard );
		original.Discards.Add( InvaderCard.Stage1( Terrain.Sands ) );
		original.ActiveSlots = [ original.Build, original.Ravage, original.Explore ]; // non-default order

		JsonObject json = original.ToJson();
		InvaderDeck restored = InvaderDeck.FromJson( json );

		restored.UnrevealedCards.Select( c => c.Code ).ShouldBe( original.UnrevealedCards.Select( c => c.Code ) );
		restored.Discards.Select( c => c.Code ).ShouldBe( original.Discards.Select( c => c.Code ) );
		restored.ActiveSlots.Select( s => s.Label ).ShouldBe( original.ActiveSlots.Select( s => s.Label ) );
		restored.Explore.Cards.Select( c => c.Code ).ShouldBe( original.Explore.Cards.Select( c => c.Code ) );
		restored.Build.Cards.Select( c => c.Code ).ShouldBe( original.Build.Cards.Select( c => c.Code ) );
		restored.Ravage.Cards.Select( c => c.Code ).ShouldBe( original.Ravage.Cards.Select( c => c.Code ) );
	}

	[Fact]
	public void HighImmegrationSlot_RoundTrips_ViaInvaderSlotRegistry() {
		// England (level 6, so _repeatWhenNoFearResolved is true) - roadmap section 9's prerequisite
		// fix: InvaderDeck.FromJson used to hardcode slot resolution to {Explore,Build,Ravage} and would
		// throw KeyNotFoundException restoring a deck whose ActiveSlots included this slot.
		var original = new SpiritIsland.Basegame.England.HighImmegrationSlot( 6 );
		original.Cards.Add( InvaderCard.Stage1( Terrain.Jungle ) );
		original.HoldNextBack();

		var slotType = typeof( SpiritIsland.Basegame.England.HighImmegrationSlot );
		var repeatField = slotType.GetField( "_repeatWhenNoFearResolved", BindingFlags.NonPublic | BindingFlags.Instance )!;
		var lastCountField = slotType.GetField( "lastCountOfFearCardsResolved", BindingFlags.NonPublic | BindingFlags.Instance )!;
		lastCountField.SetValue( original, 3 );

		JsonArray json = ( (ISerializableInvaderSlot)original ).ToJson();
		InvaderSlot restored = InvaderSlotRegistry.Deserialize( json );

		restored.ShouldBeOfType<SpiritIsland.Basegame.England.HighImmegrationSlot>();
		restored.Label.ShouldBe( "High Immigration" );
		restored.Cards.Select( c => c.Code ).ShouldBe( original.Cards.Select( c => c.Code ) );
		repeatField.GetValue( restored ).ShouldBe( true );
		lastCountField.GetValue( restored ).ShouldBe( 3 );

		// _holdBackCount survived - the one card is held back rather than advanced.
		restored.GetCardsToAdvance().ShouldBeEmpty();
	}

	[Fact]
	public async Task InvaderDeck_RoundTrips_ActiveSlots_WithEnglandsExtraSlot() {
		InvaderDeck original = InvaderDeckBuilder.Default.Build( 12345 );
		await original.InitExploreSlotAsync();

		var highImmegration = new SpiritIsland.Basegame.England.HighImmegrationSlot( 3 );
		highImmegration.Cards.Add( InvaderCard.Stage1( Terrain.Sands ) );
		original.ActiveSlots = [ highImmegration, original.Build, original.Ravage, original.Explore ];

		JsonObject json = original.ToJson();
		InvaderDeck restored = InvaderDeck.FromJson( json );

		restored.ActiveSlots.Select( s => s.Label ).ShouldBe( [ "High Immigration", "Build", "Ravage", "Explore" ] );
		var restoredExtra = restored.ActiveSlots[0].ShouldBeOfType<SpiritIsland.Basegame.England.HighImmegrationSlot>();
		restoredExtra.Cards.Select( c => c.Code ).ShouldBe( highImmegration.Cards.Select( c => c.Code ) );
	}

}
