namespace SpiritIsland;

public class InvaderPhase {

	/// <summary> Runs the whole phase start to finish. </summary>
	public static async Task ActAsync( GameState gs ) {
		var cursor = new InvaderCursor();
		while( await StepAsync( gs, cursor ) ) { }
	}

	/// <summary>
	/// Runs exactly one step of the phase and advances <paramref name="cursor"/> past it - safe to
	/// checkpoint (and save the cursor) between calls. Returns false once the phase is complete.
	/// Steps: pre-invader actions; each activated Fear card; each invader slot; advancing the deck;
	/// post-invader actions. A step that prompts the user is still one step - only the boundaries between
	/// steps are places to suspend.
	/// </summary>
	public static async Task<bool> StepAsync( GameState gs, InvaderCursor cursor ) {
		switch( cursor.Step ) {

			case InvaderStep.PreInvader:
				// Blighted Island effect
				await gs.RunPreInvaderActions();
				cursor.Step = InvaderStep.Fear;
				return true;

			case InvaderStep.Fear:
				if( !await gs.Fear.ResolveNextActivatedCard() ) {
					gs.CheckWinLoss();
					cursor.Step = InvaderStep.Slots;
				}
				return true;

			case InvaderStep.Slots:
				List<InvaderSlot> slots = gs.InvaderDeck.ActiveSlots;
				if( cursor.SlotIndex < slots.Count ) {
					await slots[cursor.SlotIndex].Execute( gs );
					gs.CheckWinLoss();
					++cursor.SlotIndex;
				} else
					cursor.Step = InvaderStep.Advance;
				return true;

			case InvaderStep.Advance:
				await gs.InvaderDeck.AdvanceAsync();
				cursor.Step = InvaderStep.PostInvader;
				return true;

			case InvaderStep.PostInvader:
				await gs.RunPostInvaderActions();
				cursor.Reset();
				return false;

			default:
				throw new InvalidOperationException( $"Unhandled InvaderStep {cursor.Step}" );
		}
	}

}

public enum InvaderStep { PreInvader, Fear, Slots, Advance, PostInvader }

/// <summary>
/// Where the Invader phase is up to. Saved alongside the game (SoloGame.Save) so a game can be resumed
/// between steps without replaying the steps already done. Back at the start once the phase completes.
/// </summary>
public class InvaderCursor {

	public InvaderStep Step { get; set; }

	/// <summary> Which of InvaderDeck.ActiveSlots runs next; only meaningful during InvaderStep.Slots. </summary>
	public int SlotIndex { get; set; }

	public void Reset() { Step = InvaderStep.PreInvader; SlotIndex = 0; }

	public JsonArray ToJson() => new( (int)Step, SlotIndex );

	public void RestoreFromJson( JsonArray json ) {
		Step = (InvaderStep)json[0]!.GetValue<int>();
		SlotIndex = json[1]!.GetValue<int>();
	}

}
