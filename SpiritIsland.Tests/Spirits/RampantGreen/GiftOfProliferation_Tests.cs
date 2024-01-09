namespace SpiritIsland.Tests.Spirits.RampantGreen;

public class GiftOfProliferation_Tests {

	#region Restrict Presence Spaces

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public async Task Ocean_CantPlacePresenceInland() {

		var setup = new ConfigurableTestFixture { Spirit = new Ocean() };

		var nonPlacableSpace = setup.GameState.Tokens[ setup.Board[8] ];

		// Given: presence on board
		SpiritExtensions.Given_Setup( setup.Spirit.Presence, nonPlacableSpace, 2 );
		nonPlacableSpace.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActionAsync( setup.TargetSelf ).AwaitUser( setup.Spirit, user => {
			// From
			user.NextDecision.HasPrompt("Select Presence to place").HasOptions("moon energy,2 cardplay,OHG").Choose("moon energy");
			// will not ask where to place because there is no valid place
		} ).ShouldComplete();

	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "HomeOfTheIslandsHeart" )]
	[Fact]
	public async Task Lure_CantPlacePresenceOnCoast() {
		var setup = new ConfigurableTestFixture { Spirit = new LureOfTheDeepWilderness() };

		var nonPlacableSpace = setup.GameState.Tokens[ setup.Board[1] ];

		// Given: presence on board
		SpiritExtensions.Given_Setup( setup.Spirit.Presence, nonPlacableSpace, 2 );
		nonPlacableSpace.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActionAsync( setup.TargetSelf ).AwaitUser( setup.Spirit, user => {
			// From
			user.NextDecision.HasPrompt("Select Presence to place")
				.HasOptions("2 energy,2 cardplay,LotDW")
				.Choose("2 energy");
			// Then: Does not contain coast/A1 as destination
			user.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A4,A5,A6").Choose("A4");
			// will not ask where to place because there is no valid place
		} ).ShouldComplete();

	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", VolcanoLoomingHigh.MountainHome )]
	[Fact]
	public async Task Volcano_CanOnlyBePlaceInMountains() {
		var setup = new ConfigurableTestFixture { Spirit = new VolcanoLoomingHigh() };

		var space = setup.GameState.Tokens[setup.Board[5]];

		// Given: presence on board
		SpiritExtensions.Given_Setup( setup.Spirit.Presence, space, 2 );
		space.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActionAsync( setup.TargetSelf ).AwaitUser( setup.Spirit, user => {
			// From
			user.NextDecision.HasPrompt("Select Presence to place")
				.HasOptions("2 energy,fire,VLH")
				.Choose("2 energy");
			// Then: does not contain non-mountain(A5) presence
			user.NextDecision.HasPrompt("Where would you like to place your presence?")
				.HasOptions("A1,A6")
				.Choose("A1");
		} ).ShouldComplete();
	}

	#endregion

}
