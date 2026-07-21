using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Solo;

/// <summary>
/// Proves SoloGame's per-action JSON rewind (Do1Action's PushGameState/PopGameState stack +
/// RewindOneAction) actually undoes gameplay when driven through the real SoloGame engine - not just
/// Spirit's own granular step methods in isolation. Only steps/calls that actually presented the user
/// with something ever leave a snapshot behind: RoundStart/EndGrowth/Invaders/TimePasses never push at
/// all, Growth pops its own push on the call where HasMoreGrowthActions turns out false (nothing shown,
/// straight through to EndGrowth), and Fast/Slow do the same via Spirit.HadNextActionOptions. _step
/// travels inside each snapshot right alongside the GameState it goes with, so a rewind restores both in
/// lockstep. The result: a rewind always lands on the *last real decision*, walking back through however
/// many silent steps ran since - including, from inside PlayCards, back past EndGrowth to whichever
/// Growth decision was last answered (not merely back to "the start of PlayCards"), and from there,
/// pressing rewind again keeps walking back through every earlier Growth decision in turn, all the way
/// to the very start of the round.
/// </summary>
public class SoloGame_Rewind_Tests {

	[Fact]
	public void Rewind_DuringGrowth_UndoesTheLastGrowthPick() {
		GameState gs = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" ).BuildGame();
		var game = new SoloGame( gs );
		game.Start();

		int energyBefore = game.Spirit.Energy;
		GrowthGroup energyGroup = game.Spirit.GrowthTrack.Groups[2]; // GainEnergy(2), PlacePresence(1)
		energyGroup.Used.ShouldBeFalse();

		// Pick "Gain 2 Energy" - resolves immediately (no targeting needed), one Do1Action call.
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).Choose( "Gain 2 Energy" );
		game.Spirit.Energy.ShouldBe( energyBefore + 2 );
		energyGroup.Used.ShouldBeTrue();

		// Next decision is for the sibling action left in that same group (still RoundStep.Growth,
		// no step transition happened) - rewind here instead of answering it.
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).HasOptions( "Place Presence(1)" );
		game.UserPortal.Rewind();

		// The "Gain 2 Energy" pick is undone.
		game.Spirit.Energy.ShouldBe( energyBefore );
		energyGroup.Used.ShouldBeFalse();

		// Engine is still alive and correct afterward - full original option set is offered again,
		// and picking it again works exactly like before the rewind.
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).Choose( "Gain 2 Energy" );
		game.Spirit.Energy.ShouldBe( energyBefore + 2 );
	}

	[Fact]
	public async Task Rewind_DuringCardPlay_WalksBackThroughEndGrowthToTheLastGrowthDecision() {
		GameState gs = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" ).BuildGame();
		Spirit spirit = gs.Spirits[0];

		spirit.Energy = 20; // affords every card in hand regardless of its real cost
		await spirit.Presence.CardPlays.Given_SlotsAreRevealed( 3 ); // VitalStrength's [Card1,Card1,Card2,...] track -> 2 plays/turn
		spirit.NumberOfCardsPlayablePerTurn.ShouldBe( 2 );

		var game = new SoloGame( gs );
		game.Start();

		int energyAtStart = game.Spirit.Energy;
		int handCountBefore = game.Spirit.Hand.Count;
		int inPlayCountBefore = game.Spirit.InPlay.Count;

		// Group 2: GainEnergy(2) (resolves immediately) + PlacePresence(1) (needs its own targeting
		// sub-decision) - picking both fully completes Growth.
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).Choose( "Gain 2 Energy" );
		game.Spirit.Energy.ShouldBe( energyAtStart + 2 );

		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).HasOptions( "Place Presence(1)" ).ChooseFirst();
		game.Spirit.NextDecision().HasPrompt( "Select Presence to place" ).ChooseFirst();

		// Growth is done, EndGrowth has run, and we're now looking at PlayCards' first decision.
		int energyBeforePlaying = game.Spirit.Energy;
		energyBeforePlaying.ShouldBeGreaterThan( energyAtStart + 2 ); // EndGrowth granted its own energy on top

		// Play 1 card...
		game.Spirit.NextDecision().HasPromptPrefix( "Play power card" ).ChooseFirst();
		game.Spirit.Energy.ShouldBeLessThan( energyBeforePlaying );
		game.Spirit.Hand.Count.ShouldBe( handCountBefore - 1 );
		game.Spirit.InPlay.Count.ShouldBe( inPlayCountBefore + 1 );

		// ...then rewind instead of playing (or declining) a second one. PlayCards' own (single,
		// whole-phase) snapshot is discarded, and the snapshot below it is the *last real Growth
		// decision* (Place Presence(1)) - not "the start of PlayCards" - since Growth's own no-decision
		// transition into EndGrowth, and EndGrowth itself, never push. So this undoes the card play,
		// EndGrowth's energy grant, AND the Place Presence(1) pick in one shot, landing back on that
		// Growth decision, unanswered, ready to be answered differently or rewound again.
		game.UserPortal.Rewind();

		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).HasOptions( "Place Presence(1)" );
		game.Spirit.Energy.ShouldBe( energyAtStart + 2 );
		game.Spirit.Hand.Count.ShouldBe( handCountBefore );
		game.Spirit.InPlay.Count.ShouldBe( inPlayCountBefore );

		// Rewind again from here (still unanswered) - walks back one more real decision, past
		// "Gain 2 Energy" too, all the way to the very start of the round.
		game.UserPortal.Rewind();

		game.Spirit.Energy.ShouldBe( energyAtStart );

		// Engine is still alive and correct afterward - the full original option set is offered again,
		// and playing all the way through to a card-play decision still works.
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).Choose( "Gain 2 Energy" );
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).HasOptions( "Place Presence(1)" ).ChooseFirst();
		game.Spirit.NextDecision().HasPrompt( "Select Presence to place" ).ChooseFirst();
		game.Spirit.NextDecision().HasPromptPrefix( "Play power card" ).ChooseFirst();
		game.Spirit.Hand.Count.ShouldBe( handCountBefore - 1 );
	}

}
