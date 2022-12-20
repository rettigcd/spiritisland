namespace SpiritIsland.Tests.Spirits.RampantGreen;

public class UnrelentingGrowth_Tests {

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

		//  And: 2 destroyed presence
		setup.Spirit.Presence.Destroyed = 2;

		//  When: Card played
		var task = UnrelentingGrowth.ActAsync( setup.TargetSelf );

		// Then: we should not be able to pick restricted space
		if( !task.IsCompleted) { // ocean has no options - so it completes
			setup.Spirit.Gateway.Next.FormatOptions().ShouldNotContain( restrictedSpace );
		}
	}

	#endregion

}
