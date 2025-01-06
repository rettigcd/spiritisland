namespace SpiritIsland.Tests.Spirits.RampantGreen;

public class GiftOfProliferation_Tests {

	#region Restrict Presence Spaces

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public async Task Ocean_CantPlacePresenceInland() {

		var gs = new SoloGameState(new Ocean());

		var nonPlacableSpace = gs.Tokens[ gs.Board[8] ];

		// Given: presence on board
		gs.Spirit.Given_IsOn( nonPlacableSpace, 2 );
		nonPlacableSpace.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActAsync( gs.Spirit.Target(gs.Spirit) ).AwaitUser( user => {
			// will not ask where to place because there is no valid place
		}).ShouldComplete();

	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "HomeOfTheIslandsHeart" )]
	[Fact]
	public async Task Lure_CantPlacePresenceOnCoast() {
		var gs = new SoloGameState(new LureOfTheDeepWilderness());
		var spirit = gs.Spirit;

		var nonPlacableSpace = gs.Tokens[ gs.Board[1] ];

		// Given: presence on board
		spirit.Given_IsOn( nonPlacableSpace, 2 );
		nonPlacableSpace.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActAsync( spirit.Target(spirit) ).AwaitUser( user => {
			// Then: Does not contain coast/A1 as destination
			user.NextDecision.HasPrompt("Select Presence to place")
				.HasFromOptions("2 energy,2 cardplay,LotDW on A1")
				.ChooseFrom("2 energy")
				.HasToOptions("A4,A5,A6")
				.ChooseTo("A4");
			// will not ask where to place because there is no valid place
		} ).ShouldComplete();

	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", VolcanoLoomingHigh.MountainHome )]
	[Fact]
	public async Task Volcano_CanOnlyBePlaceInMountains() {
		var gs = new SoloGameState(new VolcanoLoomingHigh());
		var space = gs.Tokens[gs.Board[5]];

		// Given: presence on board
		var spirit = gs.Spirit;
		spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  When: Card played
		await GiftOfProliferation.ActAsync( spirit.Target(spirit) ).AwaitUser( user => {
			// Then: does not contain non-mountain(A5) presence
			user.NextDecision.HasPrompt("Select Presence to place")
				.HasFromOptions("2 energy,fire,VLH on A5")
				.ChooseFrom("2 energy")
				.HasToOptions("A1,A6")
				.ChooseTo("A1");
		} ).ShouldComplete();
	}

	#endregion

}
