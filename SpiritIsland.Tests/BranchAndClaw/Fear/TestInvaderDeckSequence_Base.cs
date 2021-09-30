using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {

	public class TestInvaderDeckSequence_Base {

		protected TestInvaderDeckSequence_Base() {
			var powerCard = PowerCard.For<CallToTend>();
			var (userLocal,ctxLocal) = TestSpirit.SetupGame(powerCard,gs=>{ 
				gs.NewInvaderLogEntry += (s) => log.Enqueue(s);
			} );
			user = userLocal;
			ctx = ctxLocal;
			log.Clear(); // skip over initial Explorer setup
		}

		protected VirtualTestUser user;
		protected SpiritGameStateCtx ctx;
		protected Queue<string> log = new();

		protected void AdvanceToInvaderPhase() {
			ctx.ClearAllBlight();
			user.DoesNothingForARound();
		}

	}

}
