namespace SpiritIsland.Tests.Major;

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
	[Trait( "SpecialRule", VolcanoLoomingHigh.MountainHome )]
	[Fact]
	public void Volcano_CanOnlyBePlaceInMountains() {
		CantPlacePresenceHere( new VolcanoLoomingHigh(), "A5" );
	}

	static void CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {

		var gs = new SoloGameState(spirit);

		var space = gs.Tokens[gs.Board.Spaces.Single( s => s.Label == restrictedSpace )];

		// Given: presence on board
		gs.Spirit.Given_IsOn( space, 2 );
		space.Init( Token.Blight, 0 );

		//  And: 2 destroyed presence
		gs.Spirit.Presence.Destroyed.Count = 2;

		//  When: Card played
		var task = UnrelentingGrowth.ActAsync( spirit.Target(spirit) );

		if( task.IsCompleted ) return; // ocean has no options - so it completes

		// Then: we should not be able to pick restricted space
		gs.Spirit.Portal.Next.FormatOptions().ShouldNotContain(restrictedSpace);
	}

	#endregion

}
