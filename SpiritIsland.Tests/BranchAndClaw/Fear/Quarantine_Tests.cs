using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {

	public class Quarantine_Tests {

		const string FearAck1 = "Quarantine : 1 : Explore does not affect coastal lands.";
		const string FearAck2 = "Quarantine : 2 : Explore does not affect coastal lands. Lands with disease are not a source of invaders when exploring.";
		const string FearAck3 = "Quarantine : 3 : Explore does not affect coastal lands.  Invaders do not act in lands with disease.";
		readonly IFearOptions card = new Quarantine();

		public Quarantine_Tests() {
			var powerCard = PowerCard.For<CallToTend>();
			var (userLocal,ctxLocal) = TestSpirit.SetupGame(powerCard,gs=>{ 
				gs.NewLogEntry += (s) => { if(s is InvaderActionEntry) log.Enqueue(s.Msg); };
				gs.InvaderDeck = InvaderDeck.BuildTestDeck(
					new InvaderCard(Terrain.Sand), // not on coast
					InvaderCard.Stage2Costal(),
					new InvaderCard(Terrain.Jungle)
				);
			} );
			user = userLocal;
			ctx = ctxLocal;
			log.Clear(); // skip over initial Explorer setup
		}

		[Theory]
		[InlineData(false)] // 1st card is configed as costal
		[InlineData(true)]  // A1 A2 A3 are coastland and stopped by Fear
		public void Level1_ExploreDoesNotAffectCoastland( bool activateFearCard ) {

			// Given: Activate fear card
			if(activateFearCard)
				ctx.ActivateFearCard( card );

			AdvanceToInvaderPhase();

			if(activateFearCard)
				user.AcknowledgesFearCard( FearAck1 );

			log.Assert_Built( "A4", "A7" ); // Sand
			if( activateFearCard )
				log.Assert_Explored();
			else
				log.Assert_Explored( "A1","A2","A3" );
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Level2_ExploreDoesNotAffectCoastlandNorComeFromDiseasedSpots( bool activateFearCard ) {

			// Skip over the coastal build
			AdvanceToInvaderPhase();

			// The only thing around A8 (a jungle) is a diseased town
			ctx.TargetSpace("A5").Tokens.Init("");
			ctx.TargetSpace("A6").Tokens.Init("");
			ctx.TargetSpace("A7").Tokens.Init("1T@2,1Z"); // town & diZease
			ctx.TargetSpace("A8").Tokens.Init("");

			// Given: Activate fear card
			if(activateFearCard) {
				ctx.ActivateFearCard( card );
				ctx.ElevateTerrorLevelTo(2);
			}

			log.Clear();
			AdvanceToInvaderPhase();

			if(activateFearCard)
				user.AcknowledgesFearCard( FearAck2 );

			log.Assert_Ravaged("A4", "A7"); // Sand
			log.Assert_Built( "A1", "A2", "A3" ); // Costal
			if( activateFearCard )
				log.Assert_Explored(); // neither A3 (costal) nor A8 (hanging off of Diseased town) explored
			else
				log.Assert_Explored( "A3", "A8" );

		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Level3_NoCoastalExplore_NoActionInDiseasedLands( bool activateFearCard ) {

			// Skip over the coastal build
			AdvanceToInvaderPhase();

			// Ravage lands (sand:A4 & A7) have a disease
			// The only thing around A8 (a jungle) is a diseased town
			ctx.TargetSpace("A4").Tokens.Init("1E@1,1Z"); // diZease
			ctx.TargetSpace("A7").Tokens.Init("1E@1,1Z"); // diZease
			// Build lands (Costal:A1..3) all have explorers, A1 has a disease too
			ctx.TargetSpace("A1").Tokens.Init("1E@1,1Z");
			ctx.TargetSpace("A2").Tokens.Init("1E@1");
			ctx.TargetSpace("A3").Tokens.Init("1E@1");
			// Explore lands (jungle:A3 & A8) have a source (A3 is costal, A8 is town in A5)
			ctx.TargetSpace("A5").Tokens.Init("1T@2");
			ctx.TargetSpace("A8").Tokens.Init("1Z");

			// Given: Activate fear card
			if(activateFearCard) {
				ctx.ActivateFearCard( card );
				ctx.ElevateTerrorLevelTo(3);
			}

			log.Clear();
			AdvanceToInvaderPhase();

			if(activateFearCard)
				user.AcknowledgesFearCard( FearAck3 );

			if( activateFearCard) {
				log.Assert_Ravaged();            // Sand (all hvae disease)
				log.Assert_Built( "A2", "A3" );  // Coastal (A1 has disease)
				log.Assert_Explored();           // A3 is coastland, A8 has a disease
			} else {
				log.Assert_Ravaged ("A4", "A7");         // Sand
				log.Assert_Built   ( "A1", "A2", "A3" ); // Costal
				log.Assert_Explored( "A3", "A8" );
			}

		}


		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SkipRavageWorks( bool skipARavage ) {
			// Not really for quarantine, just a general test without a ghome

			// Skip over the coastal build
			AdvanceToInvaderPhase();

			// The only thing around A8 (a jungle) is a diseased town
			ctx.TargetSpace("A5").Tokens.Init("");
			ctx.TargetSpace("A6").Tokens.Init("");
			ctx.TargetSpace("A7").Tokens.Init("1T@2,1Z"); // town & diZease
			ctx.TargetSpace("A8").Tokens.Init("");

			if(skipARavage)
				ctx.GameState.SkipRavage(ctx.TargetSpace("A4").Space);

			log.Clear();
			AdvanceToInvaderPhase();

			if(skipARavage)
				log.Assert_Ravaged ( "A7" );             // Sand - A4 skipped
			else
				log.Assert_Ravaged ( "A4", "A7" );       // Sand

			log.Assert_Built   ( "A1", "A2", "A3" ); // Costal
			log.Assert_Explored( "A3", "A8" );

		}



		#region protected / private

		protected VirtualTestUser user;
		protected SelfCtx ctx;
		protected Queue<string> log = new();

		protected void AdvanceToInvaderPhase() {
			ctx.ClearAllBlight();
			user.DoesNothingForARound();
		}

		#endregion

	}

}
