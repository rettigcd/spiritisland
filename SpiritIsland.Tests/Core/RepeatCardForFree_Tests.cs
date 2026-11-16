namespace SpiritIsland.Tests.Core;

[Collection("BaseGame Spirits")]
public sealed class RepeatCardForFree_Tests {

	[Fact]
	public async Task CardResolvedTwice_IsOfferedOnce() {
		var spirit = new VitalStrength();
		var gs = new SoloGameState( spirit, Boards.A );
		gs.Phase = Phase.Fast;

		// Given: a Fast card was resolved, repeated, and resolved again - which lists it in UsedActions twice.
		var fast1 = PowerCard.ForDecorated( Fast1 );
		spirit.MarkAsResolved( fast1 );
		spirit.AddActionFactory( fast1 );
		await spirit.ResolveActionAsync( fast1, Phase.Fast );
		spirit.UsedActions.Count( a => a == fast1 ).ShouldBe( 2 );

		//  When: a second repeat effect is used
		spirit.AddActionFactory( new RepeatCardForFree( 5 ) );

		//  Then: the card is offered once, not dropped because it appears twice
		await spirit.SelectAndResolveActionsForTest( gs ).AwaitUser( user => {
			user.NextDecision.HasPrompt( "Select Fast to resolve" ).HasOptions( "Replay Card (max cost:5),Done" ).Choose( "Replay Card (max cost:5)" );
			user.NextDecision.HasPrompt( "Select card to repeat" ).HasOptions( "Fast-1 $1 (Fast),Done" ).Choose( "Done" );
		} ).ShouldComplete();
	}

	[Fact]
	public async Task CancellingTheGame_WhileChoosingTheCardToRepeat_IsNotSwallowed() {
		var spirit = new VitalStrength();
		var gs = new SoloGameState( spirit, Boards.A );
		gs.Phase = Phase.Fast;
		spirit.MarkAsResolved( PowerCard.ForDecorated( Fast1 ) );

		// Given: the action is waiting on "Select card to repeat"
		Task repeating = new RepeatCardForFree( 5 ).ActivateAsync( spirit );
		spirit.Portal.WaitForNext( 1000 ).ShouldBeTrue();
		spirit.Portal.Next!.Prompt.ShouldBe( "Select card to repeat" );

		//  When: the game is cancelled (what a withdrawal, rewind or timeout does)
		spirit.Portal.IssueException( new GameOverException( new GameOverLogEntry( GameOverResult.Withdrawal, "test" ) ) );

		//  Then: it reaches the engine instead of being ignored, so the action doesn't carry on to another decision
		await Should.ThrowAsync<GameOverException>( () => repeating );
	}

	[SpiritCard("Fast-1",1),Fast]
	[FromPresence( Filter.Ocean, 0 )] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Fast1(TargetSpaceCtx _) => Task.CompletedTask;

}
