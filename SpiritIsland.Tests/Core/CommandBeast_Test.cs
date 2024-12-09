namespace SpiritIsland.Tests;

public class CommandBeast_Test
{

	[Fact]
	public async Task AfterUsing_CanRewindToStartOfRound(){

		// Given: game using Command Beasts
		var gs = new SoloGameState(Boards.A);
		CommandBeasts.Setup(gs);
		gs.Initialize();

		// Given: Round 2
		await AdvanceToRound( gs,2 );

		//   And: "Received Command" Beasts card at end of Invader Phase
		await InvaderPhase.ActAsync(gs).AwaitUser(Acknowledge).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		// Given: Round 3
		gs.RoundNumber.ShouldBe(3);

		//   And: Saved State
		var round3Memento = ((IHaveMemento)gs).Memento;
		//   And: Used in Fast Round of Round 3
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser(UseCard).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		//  When: Reset to Round 3
		((IHaveMemento)gs).Memento = round3Memento;

		//  Then: available to use in Round 3 again.
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser(UseCard).ShouldComplete();

	}

	[Fact]
	public async Task AfterUsing_CanRewindToRound2() {

		// Given: game using Command Beasts
		var gs = new SoloGameState();
		CommandBeasts.Setup( gs );
		gs.Initialize();

		// Given: Round 2
		await AdvanceToRound( gs,2 );
		//   And: Saved State
		var round2Memento = ((IHaveMemento)gs).Memento;

		//   And: "Received Command" Beasts card at end of Invader Phase
		await InvaderPhase.ActAsync( gs ).AwaitUser( Acknowledge );
		//   And: time passes
		await gs.TriggerTimePasses();

		// Given: Round 3
		gs.RoundNumber.ShouldBe( 3 );

		//   And: Used in Fast Round of Round 3
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser( UseCard ).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		//  When: Reset to Round 2
		((IHaveMemento)gs).Memento = round2Memento;

		//  Then: "Received Command" Beasts card at end of Invader Phase (AGAIN)
		await InvaderPhase.ActAsync( gs ).AwaitUser( Acknowledge );
		//   And: time passes
		await gs.TriggerTimePasses();
		// Given: Round 3
		gs.RoundNumber.ShouldBe( 3 );
		//   And: Used in Fast Round of Round 3
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser( UseCard ).ShouldComplete();


	}

	[Fact]
	public async Task AfterUsingInRound4_AvailableInRound3(){

		// Given: game using Command Beasts
		var gs = new SoloGameState(/*new RiverSurges(), Board.BuildBoardA()*/);
		CommandBeasts.Setup(gs);
		gs.Initialize();

		// Given: Round 2
		await AdvanceToRound( gs,2 );

		//   And: "Received Command" Beasts card at end of Invader Phase
		await InvaderPhase.ActAsync(gs).AwaitUser(Acknowledge).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		// == Round 3 ==
		gs.RoundNumber.ShouldBe(3);
		//   And: Saved State
		var round3Memento = ((IHaveMemento)gs).Memento;

		// == Round 4 ==
		await AdvanceToRound( gs, 4 );

		//   And: Used in Fast
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser(UseCard).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		//  When: Reset to Round 3
		((IHaveMemento)gs).Memento = round3Memento;

		//  Then: available to use in Round 3 again.
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser(UseCard).ShouldComplete();

	}

	static async Task AdvanceToRound( GameState gs, int expectedRound ) {
		await InvaderPhase.ActAsync( gs ); gs.Given_InvadersDisappear();
		await gs.TriggerTimePasses();
		gs.RoundNumber.ShouldBe( expectedRound );
	}

	static void Acknowledge(VirtualUser user){
//		user.NextDecision.HasPrompt("Invader Deck Card Revealed").Choose("Command Beasts (II)");
	}

	static void UseCard(VirtualUser user){
		user.NextDecision.HasPrompt("Select Fast to resolve").Choose("Command Beasts (II)");
		user.NextDecision.HasPrompt("Push up to (1)").Choose("Beast on A5 => A1");
	}
}
