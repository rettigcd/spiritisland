namespace SpiritIsland.Tests.Spirits.HeartOfWildfireNS; 

public class HeartOfWildFire_Tests {

	[Trait("Blight","Destroy Presence")]
	[Fact]
	public async Task BlightAddedDueToSpiritEffects_DoesNotDestroyPresence() {
		Spirit spirit = new HeartOfTheWildfire();
		Board boardB = Boards.B;
		var gs = new SoloGameState( spirit, boardB );
		gs.IslandWontBlight();

		var space = boardB[8].ScopeSpace;

		// Given: presence on B8
		space.Init(spirit.Presence.Token,1);

		// When: adding blight to space via spirit powers
		//await using var scope = await ActionScope.Start(ActionCategory.Spirit_Power);
		//_ = LandOfHauntsAndEmbers.Act(spirit.BindMyPowers().Target(space)); // nothing to push
		await spirit.When_ResolvingCard<LandOfHauntsAndEmbers>( (user) => { 
			user.Choose(space);
		} );

		// Then: presence should still be there
		space[spirit.Presence.Token].ShouldBe(1);
		// and make sure we added blight
		space.Blight.Count.ShouldBe(1);
	}

	[Trait( "Blight", "Destroy Presence" )]
	[Fact]
	public async Task BlightAddedFromRavage_DestroysPresence() {
		Spirit spirit = new HeartOfTheWildfire();
		Board boardB = Boards.B;
		var gs = new SoloGameState( spirit, boardB );
		gs.IslandWontBlight();

		var space = boardB[8];
		var tokens = space.ScopeSpace;

		// Given: presence on B8
		tokens.Init( spirit.Presence.Token, 1 );
		// And: a town
		tokens.InitDefault(Human.Town, 1);

		// When: adding blight to space from ravage
		await tokens.SpaceSpec.When_CardRavages();

		// Then: presence should still be gone
		spirit.Presence.IsOn(tokens).ShouldBeFalse();
		// and make sure we added blight
		tokens.Blight.Count.ShouldBe( 1 );
	}


}
