namespace SpiritIsland.Tests.Minor;

public class GrowthThroughSacrifice_Tests {

	[Trait("Presence","TrackElements")]
	[Fact]
	public void RevealedTokens_GainElements() {
		var fixture = new ConfigurableTestFixture();
		SpaceSpec space = fixture.Board[3];

		// Given: Spirit's next track has an element on it (energy-air because it is Thunderspeaker)
		fixture.Spirit = new Thunderspeaker();
		Track airEnergy = Track.AirEnergy;

		//   And: has no air elements
		fixture.Spirit.Elements.Summary().ShouldBe("");
		//   And: 2 presence on board
		fixture.Spirit.Given_IsOn( fixture.GameState.Tokens[space], 2 );

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( fixture.TargetSelf );
		fixture.Choose( "Ts" ); // select presence to destroy
		fixture.Choose( space ); // select location to add presence / remove blight
		fixture.Choose( "Add 1 presence to one of your lands" );
		fixture.Choose( airEnergy ); // take presence from cardplay track

		//  Then: Spirit gains element
		fixture.Spirit.Elements.Summary().ShouldBe( "1 air" );
	}

	[Fact]
	public void RemovesBlight() {
		var setup = new ConfigurableTestFixture();

		var space = setup.GameState.Tokens[setup.Board[3]];

		// Given: 1 blight on board where presence is
		setup.Spirit.Given_IsOn( space, 2 );
		space.Init(Token.Blight,1);
		space.Blight.Count.ShouldBe( 1 );

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( setup.TargetSelf );
		setup.Choose( "RSiS" ); // select presence to destroy
		setup.Choose( space.SpaceSpec ); // select location to add presence / remove blight
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

	static async void CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {

		var setup = new ConfigurableTestFixture { Spirit = spirit };

		var space = setup.GameState.Tokens[setup.Board.Spaces.Single(s=>s.Label==restrictedSpace)];

		// Given: presence on board
		setup.Spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  When: Card played
		await GrowthThroughSacrifice.ActAsync( setup.TargetSelf).AwaitUser((u) => {
			//		setup.Choose( spirit.Presence.Token.SpaceAbreviation + " on " + space.Space ); // select presence to destroy
			setup.Choose(spirit.Presence.Token.SpaceAbreviation); // select presence to destroy
			setup.Choose(space.SpaceSpec); // select location to add presence / remove blight
		}).ShouldComplete();

	}

	#endregion
}
