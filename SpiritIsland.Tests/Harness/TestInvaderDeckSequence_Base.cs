using SpiritIsland.Log;

namespace SpiritIsland.Tests;

public class TestInvaderDeckSequence_Base {

	protected TestInvaderDeckSequence_Base() {
		var powerCard = PowerCard.For<CallToTend>();
		var (userLocal,ctxLocal) = TestSpirit.StartGame(powerCard,gs=>{ 
			gs.NewLogEntry += RecordLogItem; // (s) => log.Enqueue(s.Msg);
		} );
		user = userLocal;
		ctx = ctxLocal;
		log.Clear(); // skip over initial Explorer setup
	}

	void RecordLogItem( ILogEntry s ) {
		if(s is InvaderActionEntry or RavageEntry)
			log.Enqueue(s.Msg());
	}

	protected VirtualTestUser user;
	protected SelfCtx ctx;
	protected Queue<string> log = new();

	protected void AdvanceToInvaderPhase() {
		ctx.ClearAllBlight();
		user.AdvancesToStartOfNextInvaderPhase();
	}

}

