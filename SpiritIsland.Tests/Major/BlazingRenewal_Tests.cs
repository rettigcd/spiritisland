namespace SpiritIsland.Tests.Spirits.RampantGreen;

public class BlazingRenewal_Tests {

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

		var setup = new ConfigurableTestFixture {
			Spirit = spirit
		};

		var space = setup.GameState.Tokens[setup.Board.Spaces.Single( s => s.Text == restrictedSpace )];

		// Given: presence on board
		SpiritExtensions.Given_Adjust( setup.Spirit.Presence, space, 2 );
		space.Init( Token.Blight, 0 );

		//  And: 2 destroyed presence
		setup.Spirit.Presence.Destroyed = 2;

		//  When: Card played
		var task = BlazingRenewal.ActAsync( setup.TargetSelf );

		// Then: we should not be able to pick restricted space
		setup.Spirit.Portal.Next.FormatOptions().ShouldNotContain( restrictedSpace );
	}

	#endregion

}
