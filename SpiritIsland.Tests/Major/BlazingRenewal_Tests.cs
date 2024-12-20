namespace SpiritIsland.Tests.Major;

public class BlazingRenewal_Tests {

	#region Restrict Presence Spaces

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public Task Ocean_CantPlacePresenceInland() {
		return CantPlacePresenceHere( new Ocean(), "A8" );
	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", "HomeOfTheIslandsHeart" )]
	[Fact]
	public Task Lure_CantPlacePresenceOnCoast() {
		return CantPlacePresenceHere( new LureOfTheDeepWilderness(), "A1" );
	}

	[Trait( "Presence", "RestrictedSpace" )]
	[Trait( "SpecialRule", VolcanoLoomingHigh.MountainHome )]
	[Fact]
	public Task Volcano_CanOnlyBePlaceInMountains() {
		return CantPlacePresenceHere( new VolcanoLoomingHigh(), "A5" );
	}

	static Task CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {

		var setup = new ConfigurableTestFixture {
			Spirit = spirit
		};

		Space space = setup.GameState.Tokens[setup.Board.Spaces.Single( s => s.Label == restrictedSpace )];

		// Given: presence on board
		setup.Spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  And: 2 destroyed presence
		setup.Spirit.Presence.Destroyed.Count = 2;

		//  When: Card played
		return BlazingRenewal.ActAsync( setup.TargetSelf).AwaitUser(u => {
			// Then: we should not be able to pick restricted space
			setup.Spirit.Portal.Next.FormatOptions().ShouldNotContain(restrictedSpace);
			u.NextDecision.HasPrompt("Place 2 Destroyed Presence").ChooseFirst();
		}).ShouldComplete();
	}

	#endregion

}
