namespace SpiritIsland.Tests.Spirits.RampantGreen;

public class GiftOfProliferation_Tests {

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
	[Trait( "SpecialRule", "MountainHome" )]
	[Fact]
	public void Volcano_CanOnlyBePlaceInMountains() {
		CantPlacePresenceHere( new VolcanoLoomingHigh(), "A5" );
	}

	static void CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {

		var setup = new ConfigurableTestFixture();
		setup.Spirit = spirit;

		var space = setup.GameState.Tokens[setup.Board.Spaces.Single( s => s.Text == restrictedSpace )];

		// Given: presence on board
		setup.Spirit.Presence.Adjust( space, 2 );
		space.Init( TokenType.Blight, 0 );

		//  When: Card played
		var task = GiftOfProliferation.ActionAsync( setup.TargetSelf );

		// Then: we should not be able to pick restricted space
		var decision = setup.Spirit.Gateway.GetCurrent();
		decision.Options.Select(x=>x.Text).Join(",")
			.ShouldNotContain(restrictedSpace);
	}

	#endregion

	//  Unrelenting Growth		Range 1

}
