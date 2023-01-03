
namespace SpiritIsland.Tests.Spirits.Vengeance;

public class FieryVengeance_Tests {

	[Trait("SpecialRule", "Wreak Vengeance for the Land's Corruption" )]
	[Fact]
	public void FieryVengeance_BlightCausesBadlandDamage() {
		// Given: 2 spirits (vengeance + 1)
		Spirit vengeance = new VengeanceAsABurningPlague();
		Spirit spirit2 = new RiverSurges();
		Board board1 = Board.BuildBoardA();
		Board board2 = Board.BuildBoardB();
		GameState gameState = new GameState( new Spirit[] { vengeance, spirit2 }, new Board[] { board1, board2 } );

		//   And: spirit 2 presence, blight & town on a space
		SpaceState space = gameState.Tokens[board1[5]];
		spirit2.Presence.Adjust( space, 1 );
		space.InitTokens("1T@2");
		//   And: 1 Blight!
		space.Init( TokenType.Blight, 1 );

		//   And: spirit 2 has destroyed presence
		spirit2.Presence.Destroyed++;

		//  When: Vengencance plays FieryVengence on spirit-2
		using var uow = gameState.StartAction(ActionCategory.Spirit_Power);
		Task task = PowerCard.For<FieryVengeance>().ActivateAsync( vengeance.BindMyPowers(gameState,uow) );
		vengeance.NextDecision().HasPrompt( "Fiery Vengeance: Target Spirit" ).HasOptions( "Vengeance as a Burning Plague,River Surges in Sunlight" ).Choose( "River Surges in Sunlight" );

		//  Then: spirit 2 does 2 damage and kills town
		spirit2.NextDecision().HasPrompt("1 fear + 1 damage").HasOptions("A5").Choose("A5");
		spirit2.NextDecision().HasPrompt( "Damage (2 remaining)" ).HasOptions( "T@2" ).Choose( "T@2" );
		spirit2.NextDecision().HasPrompt( "Damage (1 remaining)" ).HasOptions( "T@1" ).Choose( "T@1" );
		space.Summary.ShouldBe("1B");

		task.IsCompleted.ShouldBeTrue();
	}

	[Fact]
	public void FieryVengeance_NoDestroyedPresence_NoAction() {
		// Given: 2 spirits (vengeance + 1)
		Spirit vengeance = new VengeanceAsABurningPlague();
		Spirit spirit2 = new RiverSurges();
		Board board1 = Board.BuildBoardA();
		Board board2 = Board.BuildBoardB();
		GameState gameState = new GameState( new Spirit[] { vengeance, spirit2 }, new Board[] { board1, board2 } );

		//   And: spirit 2 presence, blight & town on a space
		SpaceState space = gameState.Tokens[board1[5]];
		spirit2.Presence.Adjust( space, 1 );
		space.InitTokens( "1T@2" );
		//   And: 1 Blight!
		space.Init( TokenType.Blight, 1 );

		//   And: spirit 2 has NO destroyed presence
		spirit2.Presence.Destroyed = 0;

		//  When: Vengencance plays FieryVengence on spirit-2
		using var uow = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<FieryVengeance>().ActivateAsync( vengeance.BindMyPowers( gameState, uow ) );
		vengeance.NextDecision().HasPrompt( "Fiery Vengeance: Target Spirit" ).HasOptions( "Vengeance as a Burning Plague,River Surges in Sunlight" ).Choose( "River Surges in Sunlight" );

		//  Then: all done, nothing to do because can't pay Destroyed-preence cost
		task.IsCompleted.ShouldBeTrue();
	}

	// Test - Disease prevent build is optional and adds 1 fear.          (seems to be working)

}
