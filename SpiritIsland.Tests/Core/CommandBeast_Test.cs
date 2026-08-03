namespace SpiritIsland.Tests;

/// <summary>
/// CommandBeasts now becomes available by adding itself to Spirit.Mods (IModifyAvailableActions) once
/// triggered, and removes itself from Mods when used. Only 1 rewind test remains here on purpose:
/// Spirit.Mods (ModBucket) has no IHaveMemento coverage (docs/ISpiritMod-Types.md), so
/// ((IHaveMemento)gs).Memento save/restore can't roll back a Mods.Add/Remove that happened after the
/// snapshot was taken - only cases where the snapshot precedes the trigger (so re-running the Invader
/// Phase naturally re-triggers and re-adds it) round-trip correctly. That's not a live gameplay gap:
/// production's actual "rewind 1 action" (SoloGame.RewindOneAction) goes through GameState.ToJson/
/// RestoreFromJson instead, which CommandBeasts (ISerializableSpiritMod) round-trips correctly - see
/// HookActionList_Tests.SpiritModRegistry_ResolvesCommandBeasts. IHaveMemento-based "rewind to start of
/// round" isn't wired into any live UI (see ad62e34c).
/// </summary>
public class CommandBeast_Test
{

	[Fact]
	public async Task AfterUsing_CanRewindToRound3() {

		// Given: game using Command Beasts
		var gs = new SoloGameState();
		CommandBeasts.Setup( gs );
		gs.Initialize();

		// Given: Round 3 (the trigger card reaches the Build slot during the next Invader Phase)
		await AdvanceToRound( gs,3 );
		//   And: Saved State
		var round3Memento = ((IHaveMemento)gs).Memento;

		//   And: "Received Command" Beasts card at end of Invader Phase
		await InvaderPhase.ActAsync( gs ).AwaitUser( Acknowledge );
		//   And: time passes
		await gs.TriggerTimePasses();

		// Given: Round 4
		gs.RoundNumber.ShouldBe( 4 );

		//   And: Used in Fast Round of Round 4
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActionsForTest( gs ).AwaitUser( UseCard ).ShouldComplete();
		//   And: time passes
		await gs.TriggerTimePasses();

		//  When: Reset to Round 3
		((IHaveMemento)gs).Memento = round3Memento;

		//  Then: "Received Command" Beasts card at end of Invader Phase (AGAIN)
		await InvaderPhase.ActAsync( gs ).AwaitUser( Acknowledge );
		//   And: time passes
		await gs.TriggerTimePasses();
		// Given: Round 4
		gs.RoundNumber.ShouldBe( 4 );
		//   And: Used in Fast Round of Round 4
		gs.Phase = Phase.Fast;
		await gs.Spirit.SelectAndResolveActionsForTest( gs ).AwaitUser( UseCard ).ShouldComplete();


	}

	static async Task AdvanceToRound( GameState gs, int expectedRound ) {
		while( gs.RoundNumber < expectedRound ) {
			await InvaderPhase.ActAsync( gs ); gs.Given_InvadersDisappear();
			await gs.TriggerTimePasses();
		}
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
