using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests.BranchAndClaw.Fear {

	public class TestInvaderDeckSequence_Base {

		protected TestInvaderDeckSequence_Base() {
			var powerCard = PowerCard.For<CallToTend>();
			var (userLocal,ctxLocal) = TestSpirit.SetupGame(powerCard,gs=>{ 
				gs.NewLogEntry += RecordLogItem; // (s) => log.Enqueue(s.Msg);
			} );
			user = userLocal;
			ctx = ctxLocal;
			log.Clear(); // skip over initial Explorer setup
		}

		void RecordLogItem( ILogEntry s ) {
			if(s is not DecisionLogEntry)
				log.Enqueue(s.Msg);
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
