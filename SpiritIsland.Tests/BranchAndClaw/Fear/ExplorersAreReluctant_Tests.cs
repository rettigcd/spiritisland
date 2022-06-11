namespace SpiritIsland.Tests.BranchAndClaw.Fear;

public class ExplorersAreReluctant_Tests : TestInvaderDeckSequence_Base {


	[Trait( "Feature", "Explore" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Fact]
	public void NormalInvaderPhases() {

		// Disable destroying presence
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );

		AdvanceToInvaderPhase();

		log.Assert_Built( "A3", "A8" );
		log.Assert_Explored( "A2", "A5" );

		AdvanceToInvaderPhase();

		log.Assert_Ravaged( "A3", "A8" );
		log.Assert_Built( "A2", "A5" );
		log.Assert_Explored( "A4", "A7" );

		AdvanceToInvaderPhase();

		log.Assert_Ravaged( "A2", "A5" );
		log.Assert_Built( "A4", "A7" );
		log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Feature", "Explore" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Fact]
	public void Level1_SkipExploreInLowestNumberedLand() {

		// Disable destroying presence
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// 1: "During the next normal explore, skip the lowest-numbered land matching the invader card on each board.

		AdvanceToInvaderPhase();

		// Ravage:-, Build:Jungle, Explore: Wetland
		log.Assert_Built( "A3", "A8" );    // Jungle
		log.Assert_Explored( "A2", "A5" ); // Water

		// Given: Explorers Are Reluctant
		ctx.ActivateFearCard(new ExplorersAreReluctant());

		AdvanceToInvaderPhase();
		user.AcknowledgesFearCard("Explorers are Reluctant : 1 : During the next normal explore, skip the lowest-numbered land matching the invader card on each board.");
		System.Threading.Thread.Sleep(5);

		// Ravage:Jungle, Build:Wetland, Explore: Sand
		log.Assert_Ravaged( "A3", "A8" );
		log.Assert_Built( "A2", "A5" );
		log.Assert_Explored( "A7" ); // Skipped A4

		AdvanceToInvaderPhase();

		// Ravage:Wetland, Build:Sand, Explore: Jungle-2
		//log.Assert_Ravaged( "A2", "A5" );
		//log.Assert_Built( "A4", "A7" );
		//log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Feature", "Explore" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Fact]
	public void Level2_DelayExplore1Round() {

		// Disable destroying presence
		//ctx.GameState.AddBlightSideEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// 2: Skip the next normal explore.  During the next invader phase, draw an adidtional explore card.

		// Card Advance #1 - Turn up first Explore Card
		// Card Advance #2 - Advance Explore Card to Build

		AdvanceToInvaderPhase();

		log.Assert_Built( "A3", "A8" );
		log.Assert_Explored( "A2", "A5" );

		// Card Advance #3 - End of 1st round

		// Given: Explorers Are Reluctant
		ctx.ActivateFearCard( new ExplorersAreReluctant() );
		//   And: Terror Level 2
		ctx.ElevateTerrorLevelTo( 2 );

		AdvanceToInvaderPhase();
		user.AcknowledgesFearCard( "Explorers are Reluctant : 2 : Skip the next normal explore. During the next invader phase, draw an adidtional explore card." );
		System.Threading.Thread.Sleep(5);

		// Card Advance #4 - End of 2st round

		log.Assert_Ravaged( "A3", "A8" );
		log.Assert_Built( "A2", "A5" );

		AdvanceToInvaderPhase();

		log.Assert_Ravaged( "A2", "A5" );
		log.Assert_Explored( "A4", "A7" ); // A4 & A7 happen together with next
		log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

	[Trait( "Feature", "Explore" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Fact]
	public void Level3_DelayExplore1Round() {

		// Disable destroying presence
		//ctx.GameState.AddBlightSideEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

		AdvanceToInvaderPhase();

		log.Assert_Built( "A3", "A8" );
		log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		ctx.ActivateFearCard(new ExplorersAreReluctant());
		ctx.ElevateTerrorLevelTo( 3 );

		AdvanceToInvaderPhase();
		user.AcknowledgesFearCard("Explorers are Reluctant : 3 : Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.");
		System.Threading.Thread.Sleep(5);

		log.Assert_Ravaged( "A3", "A8" );
		log.Assert_Built( "A2", "A5" );
		log.Assert_Explored(); // Skipped A4 & A7

		AdvanceToInvaderPhase();

		log.Assert_Ravaged( "A2", "A5" );
		log.Assert_Built("A4", "A7"); // normal build
		log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

}