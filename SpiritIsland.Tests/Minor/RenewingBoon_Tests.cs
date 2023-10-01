namespace SpiritIsland.Tests.Minor;

public class RenewingBoon_Tests {

	[Trait("Presence","RestrictedSpace")]
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

	static async Task CantPlacePresenceHere( Spirit spirit, string restrictedSpace ) {
		// Given: Ocean
		var gameState = new GameState( spirit, Board.BuildBoardA() );

		//  And: a space that they can't place presence on
		SpaceState space = gameState.Spaces.Single( x => x.Space.Text == restrictedSpace );

		//  But: Presence already on Space (via Indomitable Claim)
		SpiritExtensions.Given_Adjust( spirit.Presence, space, 1 );
		int presenceCount = spirit.Presence.CountOn( space );

		//  And: has destroyed presence
		spirit.Presence.Destroyed = 1;

		//  And: blight on space
		space.Blight.Init( 1 );
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Spirit_Power);
		Task task = PowerCard.For<RenewingBoon>().ActivateAsync( spirit.BindMyPowers() );

		// And selecting restricted space
		spirit.Gateway.Choose( spirit.Gateway.Next, space.Space );

		// Then: it should remove any blight...
		space.Blight.Count.ShouldBe( 0 );

		//  But: no presence should be added.
		spirit.Presence.CountOn(space).ShouldBe( presenceCount );
	}

	//  Blazing Renewal			Range 2
	//  Growth Through Sacrifice	Range 0
	//  Gift of Proliferation		Range 1
	//  Unrelenting Growth		Range 1
}
