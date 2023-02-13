namespace SpiritIsland.Tests.Fear; 
public class ImmigrationSlows_Tests : TestInvaderDeckSequence_Base {

	const string FearAck1 = "Immigration Slows : 1 : During the next normal build, skip the lowest-numbered land matching the invader card on each board.";
	const string FearAck2 = "Immigration Slows : 2 : Skip the next normal build. The build card remains in place instead of shifting left.";
	const string FearAck3 = "Immigration Slows : 3 : Skip the next normal build.  The build card shifts left as usual.";
	readonly IFearCard card = new ImmigrationSlows();

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level1_SkipBuildInLowestNumberedLand() {
		// 1: During the next normal build, skip the lowest-numbered land matching the invader card on each board.

		AdvanceToInvaderPhase();

		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard( card );

		AdvanceToInvaderPhase();
		_user.AcknowledgesFearCard( FearAck1 );
		
		_ = _user.NextDecision; // wait for engine to catch up

		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2: build stopped", "A5" ); // Skipped A2
		_log.Assert_Explored( "A4", "A7" ); 

		AdvanceToInvaderPhase();

		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built( "A4", "A7" );
		_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level2_DelayBuild1Round() {
		// 2: Skip the next normal build. The build card remains in place instead of shifting left.

		// Card Advance #1 - Turn up first Explore Card
		// Card Advance #2 - Advance Explore Card to Build

		AdvanceToInvaderPhase();

		_user.WaitForNext();
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Card Advance #3 - End of 1st round
		_user.WaitForNext();

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard( card );
		//   And: Terror Level 2
		_ctx.ElevateTerrorLevelTo( 2 );

		AdvanceToInvaderPhase();
		_user.AcknowledgesFearCard( FearAck2 );
		_user.WaitForNext();

		// Card Advance #4 - End of 2st round
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Explored("A4","A7");

		AdvanceToInvaderPhase();
		_user.WaitForNext();

		// no ravage
		_log.Assert_Built( "A2", "A5" ); // double up Builds
		_log.Assert_Built( "A4", "A7" ); // double up Builds
		_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level3_DelayExplore1Round() {
		// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

		AdvanceToInvaderPhase();
		System.Threading.Thread.Sleep(5);

		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard( card );
		_ctx.ElevateTerrorLevelTo( 3 );

		AdvanceToInvaderPhase();
		_user.AcknowledgesFearCard( FearAck3 );
		System.Threading.Thread.Sleep(5);

		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Explored("A4", "A7");

		AdvanceToInvaderPhase();
		System.Threading.Thread.Sleep(5);

		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built("A4", "A7"); // normal build
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

}

