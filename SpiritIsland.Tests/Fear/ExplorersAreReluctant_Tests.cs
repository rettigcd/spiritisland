namespace SpiritIsland.Tests.Fear;

public class ExplorersAreReluctant_Tests : TestInvaderDeckSequence_Base {


	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void NormalInvaderPhases() {

		// Disable destroying presence
		_ctx.GameState.DisableBlightEffect();

		GrowAndBuyNoCards();
		_ = _user.NextDecision; // Wait for engine to finish

		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		GrowAndBuyNoCards();
		_ = _user.NextDecision; // Wait for engine to finish

		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored( "A4", "A7" );

		GrowAndBuyNoCards();
		_ = _user.NextDecision; // Wait for engine to finish

		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built( "A4", "A7" );
		_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level1_SkipExploreInLowestNumberedLand() {

		// Disable destroying presence
		_ctx.GameState.DisableBlightEffect();


		// 1: "During the next normal explore, skip the lowest-numbered land matching the invader card on each board.

		GrowAndBuyNoCards();

		_user.WaitForNext();

		// Ravage:-, Build:Jungle, Explore: Wetland
		_log.Assert_Built( "A3", "A8" );    // Jungle
		_log.Assert_Explored( "A2", "A5" ); // Water

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard(new ExplorersAreReluctant());

		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard("Explorers are Reluctant : 1 : During the next normal explore, skip the lowest-numbered land matching the invader card on each board.");
		_user.WaitForNext();

		// Ravage:Jungle, Build:Wetland, Explore: Sand
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored( "A7" ); // Skipped A4

		GrowAndBuyNoCards();

		// Ravage:Wetland, Build:Sand, Explore: Jungle-2
		//log.Assert_Ravaged( "A2", "A5" );
		//log.Assert_Built( "A4", "A7" );
		//log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level2_DelayExplore1Round() {

		// Disable destroying presence
		_ctx.GameState.DisableBlightEffect();

		// 2: Skip the next normal explore.  During the next invader phase, draw an adidtional explore card.

		// Card Advance #1 - Turn up first Explore Card
		// Card Advance #2 - Advance Explore Card to Build

		GrowAndBuyNoCards();

		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Card Advance #3 - End of 1st round

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard( new ExplorersAreReluctant() );
		//   And: Terror Level 2
		_ctx.ElevateTerrorLevelTo( 2 );

		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard( "Explorers are Reluctant : 2 : Skip the next normal explore. During the next invader phase, draw an adidtional explore card." );
		System.Threading.Thread.Sleep(5);

		// Card Advance #4 - End of 2st round

		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );

		GrowAndBuyNoCards();

		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Explored( "A4", "A7" ); // A4 & A7 happen together with next
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level3_DelayExplore1Round() {

		// Disable destroying presence
		_ctx.GameState.DisableBlightEffect();

		// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

		// When Round 1 comes and goes
		GrowAndBuyNoCards(); // Begin Round 1
		_user.WaitForNext(); // End Round 1

		// Then:
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard(new ExplorersAreReluctant());
		_ctx.ElevateTerrorLevelTo( 3 );

		// When Round 2 comes and goes
		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard("Explorers are Reluctant : 3 : Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.");
		_user.WaitForNext();

		// Then:
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored(); // Skipped A4 & A7

		// When: Round 3 comes and goes
		GrowAndBuyNoCards();
		_user.WaitForNext();

		// Then:
		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built(/*"A4", "A7"*/); // we are currently filtering out spaces that have no explorers so they don't show up here.
		_log.Dequeue().ShouldStartWith( "No build due" );
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

}