﻿using SpiritIsland.BranchAndClaw;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {

	public class ExplorersAreReluctant_Tests : TestInvaderDeckSequence_Base {

		[Fact]
		public void NormalInvaderPhases() {

			AdvanceToInvaderPhase();

			Assert_Ravaged();
			Assert_Built( "A3", "A8" );
			Assert_Explored( "A2", "A5" );

			AdvanceToInvaderPhase();

			Assert_Ravaged( "A3", "A8" );
			Assert_Built( "A2", "A5" );
			Assert_Explored( "A4", "A7" );

			AdvanceToInvaderPhase();

			Assert_Ravaged( "A2", "A5" );
			Assert_Built( "A4", "A7" );
			Assert_Explored( "A3", "A8" );

		}

		[Fact]
		public void Level1_SkipExploreInLowestNumberedLand() {
			// 1: "During the next normal explore, skip the lowest-numbered land matching the invader card on each board.

			AdvanceToInvaderPhase();

			Assert_Ravaged();
			Assert_Built( "A3", "A8" );
			Assert_Explored( "A2", "A5" );

			// Given: Explorers Are Reluctant
			ActivateFearCard(new ExplorersAreReluctant());

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard("Explorers are Reluctant:1:During the next normal explore, skip the lowest-numbered land matching the invader card on each board.");

			Assert_Ravaged( "A3", "A8" );
			Assert_Built( "A2", "A5" );
			Assert_Explored( "A7" ); // Skipped A4

			AdvanceToInvaderPhase();

			Assert_Ravaged( "A2", "A5" );
			Assert_Built( "A4", "A7" );
			Assert_Explored( "A3", "A8" );

		}

		[Fact]
		public void Level2_DelayExplore1Round() {
			// 2: Skip the next normal explore.  During the next invader phase, draw an adidtional explore card.

			// Card Advance #1 - Turn up first Explore Card
			// Card Advance #2 - Advance Explore Card to Build

			AdvanceToInvaderPhase();

			Assert_Ravaged();
			Assert_Built( "A3", "A8" );
			Assert_Explored( "A2", "A5" );

			// Card Advance #3 - End of 1st round

			// Given: Explorers Are Reluctant
			ActivateFearCard( new ExplorersAreReluctant() );
			//   And: Terror Level 2
			ElevateTerrorLevelTo( 2 );

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard( "Explorers are Reluctant:2:Skip the next normal explore.  During the next invader phase, draw an adidtional explore card." );

			// Card Advance #4 - End of 2st round

			Assert_Ravaged( "A3", "A8" );
			Assert_Built( "A2", "A5" );
			Assert_Explored(); // Skipped A4 & A7

			AdvanceToInvaderPhase();

			Assert_Ravaged( "A2", "A5" );
			Assert_Built(); // no build
			Assert_Explored( "A3", "A4", "A7", "A8" ); // A4 & A7 happen together with next

		}

		[Fact]
		public void Level3_DelayExplore1Round() {
			// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

			AdvanceToInvaderPhase();

			Assert_Ravaged();
			Assert_Built( "A3", "A8" );
			Assert_Explored( "A2", "A5" );

			// Given: Explorers Are Reluctant
			ActivateFearCard(new ExplorersAreReluctant());
			ElevateTerrorLevelTo( 3 );

			AdvanceToInvaderPhase();
			user.AcknowledgesFearCard("Explorers are Reluctant:3:Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.");

			Assert_Ravaged( "A3", "A8" );
			Assert_Built( "A2", "A5" );
			Assert_Explored(); // Skipped A4 & A7

			AdvanceToInvaderPhase();

			Assert_Ravaged( "A2", "A5" );
			Assert_Built("A4", "A7"); // normal build
			Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

		}

	}


}
