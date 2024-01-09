namespace SpiritIsland.Tests.Minor;

public class GrowthThroughSacrifice_Tests {

	[Trait("Presence","TrackElements")]
	[Fact]
	public void RevealedTokens_GainElements() {
		var fixture = new ConfigurableTestFixture();
		var elementTrack = Track.AirEnergy;
		var space = fixture.Board[3];

		// Given: Spirit's next cardplay has an element on it
		fixture.CardPlayTrack = new PresenceTrack( Track.Card1, elementTrack );
		//   And: has no air elements
		fixture.Spirit.Elements.BuildElementString().ShouldBe("");
		//   And: 2 presence on board
		SpiritExtensions.Given_Setup( fixture.Spirit.Presence, fixture.GameState.Tokens[space], 2 );

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( fixture.TargetSelf );
		fixture.Choose( "CS" ); // select presence to destroy
		fixture.Choose( space ); // select location to add presence / remove blight
		fixture.Choose( "Add 1 presence to one of your lands" );
		fixture.Choose( elementTrack ); // take presence from cardplay track

		//  Then: Spirit gains element
		fixture.Spirit.Elements.BuildElementString().ShouldBe( "1 air" );
	}

	[Fact]
	public void RemovesBlight() {
		var setup = new ConfigurableTestFixture();

		var space = setup.GameState.Tokens[setup.Board[3]];

		// Given: 1 blight on board where presence is
		SpiritExtensions.Given_Setup( setup.Spirit.Presence, space, 2 );
		space.Init(Token.Blight,1);
		space.Blight.Count.ShouldBe( 1 );

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( setup.TargetSelf );
		setup.Choose( "CS" ); // select presence to destroy
		setup.Choose( space.Space ); // select location to add presence / remove blight
		setup.Choose( "Remove 1 blight from one of your lands" );

		//  Then: Spirit gains element
		space.Blight.Count.ShouldBe(0);

	}

	#region Restrict Presence Spaces

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public void Ocean_CantPlacePresenceInland() {
		CantPlacePresenceHere( new Ocean(), "A8" );
	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "HomeOfTheIslandsHeart" )]
	[Fact]
	public void Lure_CantPlacePresenceOnCoast() {
		CantPlacePresenceHere( new LureOfTheDeepWilderness(), "A1" );
	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", VolcanoLoomingHigh.MountainHome )]
	[Fact]
	public void Volcano_CanOnlyBePlaceInMountains() {
		CantPlacePresenceHere( new VolcanoLoomingHigh(), "A5" );
	}

	static void CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {

		var setup = new ConfigurableTestFixture { Spirit = spirit };

		var space = setup.GameState.Tokens[setup.Board.Spaces.Single(s=>s.Text==restrictedSpace)];

		// Given: presence on board
		SpiritExtensions.Given_Setup( setup.Spirit.Presence, space, 2 );
		space.Init( Token.Blight, 0 );

		//  When: Card played
		var task = GrowthThroughSacrifice.ActAsync( setup.TargetSelf );
//		setup.Choose( spirit.Presence.Token.SpaceAbreviation + " on " + space.Space ); // select presence to destroy
		setup.Choose( spirit.Presence.Token.SpaceAbreviation); // select presence to destroy
		setup.Choose( space.Space ); // select location to add presence / remove blight

		// Then: there should be nothing to do because spirit can't add stuff there.
		task.IsCompleted.ShouldBeTrue();
	}

	#endregion
}
