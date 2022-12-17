namespace SpiritIsland.Tests.Minor;

public class GrowthThroughSacrifice_Tests {

	[Fact]
	public void RevealedTokens_GainElements() {
		var fixture = new ConfigurableTestFixture();
		var elementTrack = Track.AirEnergy;
		var space = fixture.Board[3];

		// Given: Spirit's next cardplay has an element on it
		fixture.CardPlayTrack = new PresenceTrack( Track.Card1, elementTrack );
		//   And: has no air elements
		fixture.Spirit.Elements[elementTrack.Elements[0]].ShouldBe(0);
		//   And: 2 presence on board
		fixture.Spirit.Presence.Adjust( fixture.GameState.Tokens[space], 2);

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( fixture.TargetSelf );
		fixture.Choose( space ); // select presence to destroy
		fixture.Choose( space ); // select location to add presence / remove blight
		fixture.Choose( "add 1 presence" );
		fixture.Choose( elementTrack ); // take presence from cardplay track

		//  Then: Spirit gains element
		fixture.Spirit.Elements[ elementTrack.Elements[0] ].ShouldBe( 1 );

	}

	[Fact]
	public void RemovesBlight() {
		var setup = new ConfigurableTestFixture();
		var space = setup.Board[3];
		var tokens = setup.GameState.Tokens[space];

		// Given: 1 blight on board where presence is
		setup.Spirit.Presence.Adjust( tokens, 2 );
		tokens.Init(TokenType.Blight,1);
		tokens.Blight.Count.ShouldBe( 1 );

		//  When: Card played
		_ = GrowthThroughSacrifice.ActAsync( setup.TargetSelf );
		setup.Choose( space ); // select presence to destroy
		setup.Choose( space ); // select location to add presence / remove blight
		setup.Choose( "blight" );

		//  Then: Spirit gains element
		tokens.Blight.Count.ShouldBe(0);

	}


}