using SpiritIsland.BranchAndClaw;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {

	public class ImmigrationSlows_Tests : TestInvaderDeckSequence_Base {

		const string FearAck1 = "Immigration Slows : 1 : During the next normal build, skip the lowest numbered land matching the invader card on each board.";
		const string FearAck2 = "Immigration Slows : 2 : Skip the next normal build. The build card remains in place instead of shifting left.";
		const string FearAck3 = "Immigration Slows : 3 : Skip the next normal build.  The build card shifts left as usual.";
		readonly IFearOptions card = new ImmigrationSlows();

		[Fact]
		public void Level1_SkipBuildInLowestNumberedLand() {
			// 1: During the next normal build, skip the lowest numbered land matching the invader card on each board.

			AdvanceToInvaderPhase();

//			log.Assert_Ravaged();
			log.Assert_Built( "A3", "A8" );
			log.Assert_Explored( "A2", "A5" );

			// Given: Explorers Are Reluctant
			ctx.ActivateFearCard( card );

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard( FearAck1 );
			System.Threading.Thread.Sleep(5);

			log.Assert_Ravaged( "A3", "A8" );
			log.Assert_Built( "A5" ); // Skipped A2
			log.Assert_Explored( "A4", "A7" ); 

			AdvanceToInvaderPhase();

			log.Assert_Ravaged( "A2", "A5" );
			log.Assert_Built( "A4", "A7" );
			log.Assert_Explored( "A3", "A8" );

		}

		[Fact]
		public void Level2_DelayBuild1Round() {
			// 2: Skip the next normal build. The build card remains in place instead of shifting left.

			// Card Advance #1 - Turn up first Explore Card
			// Card Advance #2 - Advance Explore Card to Build

			AdvanceToInvaderPhase();

			log.Assert_Built( "A3", "A8" );
			log.Assert_Explored( "A2", "A5" );

			// Card Advance #3 - End of 1st round

			// Given: Explorers Are Reluctant
			ctx.ActivateFearCard( card );
			//   And: Terror Level 2
			ctx.ElevateTerrorLevelTo( 2 );

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard( FearAck2 );
			System.Threading.Thread.Sleep(5);

			// Card Advance #4 - End of 2st round

			log.Assert_Ravaged( "A3", "A8" );
			log.Assert_Built(); // Skip A2 & A5
			log.Assert_Explored("A4","A7");

			AdvanceToInvaderPhase();
			System.Threading.Thread.Sleep(5);

			// no ravage
			log.Assert_Built( "A2", "A5" ); // double up Builds
			log.Assert_Built( "A4", "A7" ); // double up Builds
			log.Assert_Explored( "A3", "A8" );

		}

		[Fact]
		public void Level3_DelayExplore1Round() {
			// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

			AdvanceToInvaderPhase();
			System.Threading.Thread.Sleep(5);

			log.Assert_Built( "A3", "A8" );
			log.Assert_Explored( "A2", "A5" );

			// Given: Explorers Are Reluctant
			ctx.ActivateFearCard( card );
			ctx.ElevateTerrorLevelTo( 3 );

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard( FearAck3 );
			System.Threading.Thread.Sleep(5);

			log.Assert_Ravaged( "A3", "A8" );
			log.Assert_Built(); // Skipped A2 & A5
			log.Assert_Explored("A4", "A7");

			AdvanceToInvaderPhase();
			System.Threading.Thread.Sleep(5);

			log.Assert_Ravaged( "A2", "A5" );
			log.Assert_Built("A4", "A7"); // normal build
			log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

		}

	}

}
