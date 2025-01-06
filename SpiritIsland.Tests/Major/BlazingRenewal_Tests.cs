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

		var gs = new SoloGameState(spirit, Boards.A);
		Space space = gs.Board.Spaces.Single( s=>s.Label==restrictedSpace).ScopeSpace;

		// Given: presence on board
		spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  And: 2 destroyed presence
		spirit.Presence.Destroyed.Count = 2;

		//  When: Card played
		return BlazingRenewal.ActAsync( spirit.Target(spirit) ).AwaitUser(u => {
			// Then: we should not be able to pick restricted space
			spirit.Portal.Next.FormatOptions().ShouldNotContain(restrictedSpace);
			u.NextDecision.HasPrompt("Place 2 Destroyed Presence").ChooseFirst();
		}).ShouldComplete();
	}

	#endregion

}
