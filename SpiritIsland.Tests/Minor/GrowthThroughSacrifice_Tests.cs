namespace SpiritIsland.Tests.Minor;

public class GrowthThroughSacrifice_Tests {

	[Trait("Presence","TrackElements")]
	[Fact]
	public async Task RevealedTokens_GainElements() {
		var spirit = new Thunderspeaker();
		var gs = new SoloGameState(spirit, Boards.A);
		Space space = gs.Board[3].ScopeSpace;
		IOption airEnergy = Track.AirEnergy;

		// Given: Spirit's next track has an element on it (energy-air because it is Thunderspeaker)

		//   And: has no air elements
		spirit.Elements.Summary().ShouldBe("");
		//   And: 2 presence on board
		spirit.Given_IsOn( space, 2 );

		//  When: Card played
		await GrowthThroughSacrifice.ActAsync( spirit.Target(spirit) ).AwaitUser(user => {
			user.NextDecision.Choose("Ts"); // select presence to destroy
			user.NextDecision.Choose(space.Label); // select location to add presence / remove blight
			user.NextDecision.Choose("Add 1 presence to one of your lands");
			user.NextDecision.Choose(airEnergy.Text); // take presence from cardplay track
		}).ShouldComplete();

		//  Then: Spirit gains element
		spirit.Elements.Summary().ShouldBe( "1 air" );
	}

	[Fact]
	public async Task RemovesBlight() {
		var gs = new SoloGameState();

		var space = gs.Tokens[gs.Board[3]];

		// Given: 1 blight on board where presence is
		gs.Spirit.Given_IsOn( space, 2 );
		var spirit = gs.Spirit;
		space.Init(Token.Blight,1);
		space.Blight.Count.ShouldBe( 1 );

		//  When: Card played
		await GrowthThroughSacrifice.ActAsync( spirit.Target(spirit) ).AwaitUser(user => {
			user.NextDecision.Choose("TS"); // select presence to destroy
			user.NextDecision.Choose(space.Label); // select location to add presence / remove blight
			user.NextDecision.Choose("Remove 1 blight from one of your lands");
		}).ShouldComplete();

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

		var gs = new SoloGameState(spirit);

		var space = gs.Tokens[gs.Board.Spaces.Single(s=>s.Label==restrictedSpace)];

		// Given: presence on board
		gs.Spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  When: Card played
		await GrowthThroughSacrifice.ActAsync( spirit.Target(spirit) ).AwaitUser(user => {
			//		setup.Choose( spirit.Presence.Token.SpaceAbreviation + " on " + space.Space ); // select presence to destroy
			user.NextDecision.Choose(spirit.Presence.Token.SpaceAbreviation); // select presence to destroy
			user.NextDecision.Choose(space.Label); // select location to add presence / remove blight
		}).ShouldComplete();

	}

	#endregion
}
