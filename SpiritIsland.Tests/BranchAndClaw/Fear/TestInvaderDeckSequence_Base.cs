using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {
	public class TestInvaderDeckSequence_Base {

		protected TestInvaderDeckSequence_Base() {
			var (userLocal,ctxLocal) = TestSpirit.SetupGame(PowerCard.For<CallToTend>(),gs=>{ 
				gs.NewInvaderLogEntry += (s) => log.Add(s);
			} );
			user = userLocal;
			ctx = ctxLocal;
		}

		protected VirtualTestUser user;
		protected SpiritGameStateCtx ctx;
		protected List<string> log = new();
		protected int logIndex;

		protected void ActivateFearCard(IFearOptions fearCard) {
			ctx.GameState.Fear.Deck.Pop();
			ctx.GameState.Fear.ActivatedCards.Push( new PositionFearCard{ FearOptions=fearCard, Text="FearCard" } );
		}

		protected void AdvanceToInvaderPhase() {
			ResetLog();
			ctx.ClearAllBlight();
			user.DoesNothingForARound();
			System.Threading.Thread.Sleep( 5 );
		}

		protected void Assert_Built( params string[] spaces ) {
			System.Threading.Thread.Sleep(10);

			log[logIndex++].ShouldStartWith( "Building" );
			foreach(var s in spaces)
				log[logIndex++].ShouldStartWith( s );
		}

		protected void Assert_Explored( params string[] spaces ) {
			System.Threading.Thread.Sleep(10);
			if(logIndex + spaces.Length>log.Count)
				throw new System.Exception("Not enough log entries.:" + log.Skip(logIndex).Join(" -- "));

			log[logIndex++].ShouldStartWith( "Exploring" );
			foreach(var s in spaces)
				log[logIndex++].ShouldStartWith( s );
		}

		protected void Assert_Ravaged( params string[] spaces ) {
			System.Threading.Thread.Sleep(10);

			log[logIndex++].ShouldStartWith( "Ravaging" );
			foreach(var s in spaces)
				log[logIndex++].ShouldStartWith( s );
		}

		protected void ElevateTerrorLevelTo( int desiredFearLevel ) {
			while(ctx.GameState.Fear.TerrorLevel < desiredFearLevel)
				ctx.GameState.Fear.Deck.Pop();
		}

		protected void ResetLog() {
			log.Clear(); logIndex = 0;
		}
	}


}
