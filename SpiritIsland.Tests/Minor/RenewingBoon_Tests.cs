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
		var gameState = new SoloGameState( spirit, Boards.A );

		//  And: a space that they can't place presence on
		Space space = ActionScope.Current.Spaces.Single( x => x.Label == restrictedSpace );

		//  But: Presence already on Space (via Indomitable Claim)
		spirit.Given_IsOn( space );
		int presenceCount = spirit.Presence.CountOn( space );

		//  And: has destroyed presence
		spirit.Presence.Destroyed.Count = 1;

		//  And: blight on space
		space.Blight.Init( 1 );
		await using ActionScope actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, spirit);

		// When: playing Renewing boon
		await PowerCard.For(typeof(RenewingBoon)).ActivateAsync( spirit ).AwaitUser( u => {
			// And selecting restricted space
			u.NextDecision.Choose(space.SpaceSpec);
		}).ShouldComplete();

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
