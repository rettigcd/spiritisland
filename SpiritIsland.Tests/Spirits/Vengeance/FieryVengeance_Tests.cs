namespace SpiritIsland.Tests.Spirits.Vengeance;

public class FieryVengeance_Tests {

	[Trait("SpecialRule", "Wreak Vengeance for the Land's Corruption" )]
	[Fact]
	public async Task FieryVengeance_BlightCausesBadlandDamage() {

		var gameState = new GameConfiguration()
			.ConfigSpirits(VengeanceAsABurningPlague.Name,RiverSurges.Name)
			.ConfigBoards("A","B")
			.BuildShell();
		Spirit vengeance = (VengeanceAsABurningPlague)gameState.Spirits[0];
		Spirit spirit2 = (RiverSurges)gameState.Spirits[1];
		Board board1 = gameState.Island.Boards[0];
		Board board2 = gameState.Island.Boards[1];

		//   And: spirit 2 presence, blight & town on a space
		Space space = gameState.Tokens[board1[5]];
		spirit2.Given_IsOn( space );
		space.Given_HasTokens("1T@2,1B");

		//   And: spirit 2 has destroyed presence
		spirit2.Presence.Destroyed.Count++;

		//  When: Vengencance plays FieryVengence on spirit-2
		await vengeance.When_ResolvingCard<FieryVengeance>( (user) => {
			user.NextDecision.HasPrompt( "Fiery Vengeance: Target Spirit" ).HasOptions( "Vengeance as a Burning Plague,River Surges in Sunlight" ).Choose( "River Surges in Sunlight" );

			//  Then: spirit 2 does 2 damage and kills town
			VirtualUser user2 = new VirtualUser(spirit2);
			user2.NextDecision.HasPrompt( "1 fear + 1 damage" ).HasOptions( "A5" ).Choose( "A5" );
			user2.NextDecision.HasPrompt( "Damage (2 remaining)" ).HasOptions( "T@2" ).Choose( "T@2" );
			user2.NextDecision.HasPrompt( "Damage (1 remaining)" ).HasOptions( "T@1" ).Choose( "T@1" );
		} );

		space.Summary.ShouldBe("1B,1RSiS");

	}

	[Fact]
	public async Task FieryVengeance_NoDestroyedPresence_NoAction() {
		// Given: 2 spirits (vengeance + 1)
		var gs = new GameConfiguration()
			.ConfigSpirits(VengeanceAsABurningPlague.Name,RiverSurges.Name)
			.ConfigBoards("A","B")
			.BuildShell();
		Spirit vengeance = (VengeanceAsABurningPlague)gs.Spirits[0];
		Spirit spirit2 = (RiverSurges)gs.Spirits[1];
		Board board1 = gs.Island.Boards[0];
		Board board2 = gs.Island.Boards[1];

		//   And: spirit 2 presence, blight & town on a space
		Space space = gs.Tokens[board1[5]];
		spirit2.Given_IsOn( space );
		space.Given_HasTokens( "1T@2,1B" );

		//   And: spirit 2 has NO destroyed presence
		spirit2.Presence.Destroyed.Count = 0;

		//  When: Vengencance plays FieryVengence on spirit-2
		await vengeance.When_ResolvingCard<FieryVengeance>( (user) => {
			user.NextDecision.HasPrompt( "Fiery Vengeance: Target Spirit" )
				.HasOptions( "Vengeance as a Burning Plague,River Surges in Sunlight" )
				.Choose( "River Surges in Sunlight" );
		} );

	}

	// Test - Disease prevent build is optional and adds 1 fear.          (seems to be working)

}
