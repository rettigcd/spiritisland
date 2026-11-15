namespace SpiritIsland.SinglePlayer;

public class SoloGame {

	/// <summary> The main interface that drives the UI</summary>
	public IGamePortal UserPortal {get; set;}

	// Public for querying gamestate ad hoc
	public GameState GameState { get; }

	// simplies accessing the only spirit playing
	public Spirit Spirit {get; set;}

	public bool LogExceptions { get; set; }

	/// <summary>
	/// How long the engine waits on any one decision before giving up: it rolls back to its last checkpoint
	/// and AdvanceAsync/ChooseAsync return <see cref="MenuKind.Suspended"/> carrying the save. Null (the
	/// default) waits forever.
	/// </summary>
	public TimeSpan? PromptTimeout { get => Spirit.PromptTimeout; set => Spirit.PromptTimeout = value; }

	// User preferences
	public bool EnablePreselects; // Tokens and spaces together

	#region constructor

	public SoloGame(GameState gameState){
		GameState = gameState;
		Spirit = gameState.Spirits.Single(); // this player only handles single-player.
		UserPortal = new GamePortal( Spirit.Portal );
	}


	#endregion

	/// <summary>
	/// Starts game engine (asynchronously) and saves the Task to .EngineTask
	/// </summary>
	public void Start() {
		// this MUST be called at least once on the UI thread so that we can query the GameState
		ActionScope.Initialize( GameState.RootScope );
		EngineTask = StartAsync();
	}

	public Task? EngineTask { get; private set; }

	/// <summary>
	/// Whether Rewind() has something real to undo. Exactly 1 entry means it's the state from before the
	/// very first decision of the round - rewinding from there is a no-op (nothing earlier exists to
	/// distinguish it from), so it doesn't count as "can rewind" even though the stack isn't empty.
	/// </summary>
	public bool CanRewind => 1 < _stateStack.Count;

	/// <summary>
	/// Runs the whole game as one long-lived task: a loop over the stepwise core below that asks the user
	/// (through Spirit's decision gateway) to pick from each <see cref="PendingMenu"/>. Callers that want to
	/// suspend between actions should drive <see cref="AdvanceAsync"/>/<see cref="ChooseAsync"/> directly.
	/// </summary>
	public async Task StartAsync() {

		UserGateway.UsePreselect.Value = EnablePreselects;
		try {
			PendingMenu? menu = null;
			while( true ) {
				try {
					menu ??= await AdvanceAsync();
					if( menu.Kind is MenuKind.GameOver or MenuKind.Suspended ) break;

					_stateStack.Add( _checkpoint! ); // the state as of this menu - what a Rewind returns to
					int choice = await UserPicksAsync( menu );
					menu = await ChooseAsync( choice );
				}
				catch( RewindException rewind ) {
					RewindOneAction( rewind );
					menu = null; // re-derive the menu from the restored state
				}
				catch( PromptTimeoutException ) { // timed out choosing from the menu itself - state is clean
					Suspended = Save();
					break;
				}
			}
		}
		catch(Exception ex) {
			GameState.Log(new Log.ExceptionEntry( ex ) );
		}

	}

	/// <summary>
	/// Undoes the action that just threw: discards the snapshot taken at its start (the last one pushed;
	/// guarded, since if it's the only entry we still need to read it below), then consumes (restores
	/// from, and always also pops - a fresh replacement is about to be pushed immediately by the retry,
	/// so there's never a reason to leave the old one behind as a dead duplicate) the snapshot from
	/// before that - the state as of the last successfully-completed undo-able step - so the next
	/// menu is retried fresh from there. If only one snapshot exists (the very first action of
	/// the game), the first discard is skipped so there's still something to restore from - it's reload
	/// (not discard) that makes it a no-op, not leaving the entry behind afterward.
	/// </summary>
	void RewindOneAction( RewindException rex ) {
		if( 1 < _stateStack.Count )
			_stateStack.RemoveAt( _stateStack.Count - 1 );

		Load( _stateStack[^1] );

		_stateStack.RemoveAt( _stateStack.Count - 1 );

		// Do this last so anything that triggers off of this, has new game state.
		GameState.Log(rex);
	}

	#region Stepwise core

	enum RoundStep { RoundStart, Growth, EndGrowth, PlayCards, Fast, Invaders, Slow, TimePasses, Init }

	// Init (ocean/beast placement) runs once, before the first round.
	RoundStep _step = RoundStep.Init;

	// Position within the Invader phase; only meaningful while _step is Invaders.
	readonly InvaderCursor _invader = new();

	/// <summary>
	/// Runs every step that needs nothing from the user (and any that prompt only for sub-decisions of an
	/// action - Invader-phase fear cards, presence-track icons) until the game needs the user to pick the
	/// next action, then returns what they can pick from. Safe to call repeatedly, and right after
	/// <see cref="Load"/>: when the game is already waiting on a menu it changes nothing and just returns it.
	/// </summary>
	public async Task<PendingMenu> AdvanceAsync() {
		try {
			while( true ) {
				switch( _step ) {

					case RoundStep.Init:
						GameState.Phase = Phase.Init;
						IActionFactory[] initMenu = Spirit.GetActionMenu( Phase.Init );
						if( 0 < initMenu.Length ) return Menu( MenuKind.Init, initMenu );
						_step = RoundStep.RoundStart;
						break;

					case RoundStep.RoundStart:
						LogRound();
						SetPhase( Phase.Growth );
						Spirit.GrowthTrack.Reset();
						_step = RoundStep.Growth;
						break;

					case RoundStep.Growth:
						if( Spirit.HasMoreGrowthActions )
							return Menu( MenuKind.Growth, Spirit.GetGrowthMenu() );
						_step = RoundStep.EndGrowth;
						break;

					case RoundStep.EndGrowth:
						NewCheckpoint();
						await Spirit.EndGrowth();
						_step = RoundStep.PlayCards;
						break;

					case RoundStep.PlayCards:
						// Picking all the cards is one grouped action, so it has no option list of its own.
						return Menu( MenuKind.PlayCards, [] );

					case RoundStep.Fast:
						IActionFactory[] fastMenu = Spirit.GetActionMenu( Phase.Fast );
						if( 0 < fastMenu.Length ) return Menu( MenuKind.Fast, fastMenu );
						EndFast();
						break;

					case RoundStep.Invaders:
						// One checkpoint per step, so a timeout rolls back only the step that was prompting and
						// the user's answers to earlier steps (e.g. an earlier Fear card) are kept.
						NewCheckpoint();
						while( await InvaderPhase.StepAsync( GameState, _invader ) )
							NewCheckpoint();
						SetPhase( Phase.Slow );
						_step = RoundStep.Slow;
						break;

					case RoundStep.Slow:
						IActionFactory[] slowMenu = Spirit.GetActionMenu( Phase.Slow );
						if( 0 < slowMenu.Length ) return Menu( MenuKind.Slow, slowMenu );
						_step = RoundStep.TimePasses;
						break;

					case RoundStep.TimePasses:
						NewCheckpoint();
						await GameState.TriggerTimePasses();
						_step = RoundStep.RoundStart;
						break;

					default:
						throw new InvalidOperationException( $"Unhandled RoundStep {_step}" );
				}
			}
		}
		catch( GameOverException gameOver ) {
			return GameOver( gameOver );
		}
		catch( PromptTimeoutException ) {
			return Suspend();
		}
	}

	/// <summary>
	/// Resolves the user's pick from the menu <see cref="AdvanceAsync"/> last returned - the only part that
	/// has to run as an async process - then advances to the next menu. <paramref name="index"/> is the
	/// position in <see cref="PendingMenu.Options"/>, or <see cref="Done"/> to finish a Fast/Slow phase.
	/// The menu is re-derived from game state rather than remembered, so this works on a freshly Loaded game.
	/// </summary>
	public async Task<PendingMenu> ChooseAsync( int index ) {
		_checkpoint = Save(); // game is sitting at the menu - what a PromptTimeout inside the action rolls back to
		try {
			switch( _step ) {

				case RoundStep.Init:
					await Spirit.ResolveActionAsync( Spirit.GetActionMenu( Phase.Init )[index], Phase.Init );
					break;

				case RoundStep.Growth:
					await Spirit.ResolveGrowthActionAsync( Spirit.GetGrowthMenu()[index] );
					break;

				case RoundStep.PlayCards:
					await Spirit.SelectAndPlayCardsFromHand();
					SetPhase( Phase.Fast );
					_step = RoundStep.Fast;
					break;

				case RoundStep.Fast:
					if( index == Done ) EndFast();
					else await Spirit.ResolveActionAsync( Spirit.GetActionMenu( Phase.Fast )[index], Phase.Fast );
					break;

				case RoundStep.Slow:
					if( index == Done ) _step = RoundStep.TimePasses;
					else await Spirit.ResolveActionAsync( Spirit.GetActionMenu( Phase.Slow )[index], Phase.Slow );
					break;

				default:
					throw new InvalidOperationException( $"RoundStep {_step} is not waiting for a choice." );
			}
		}
		catch( GameOverException gameOver ) {
			return GameOver( gameOver );
		}
		catch( PromptTimeoutException ) {
			return Suspend();
		}
		return await AdvanceAsync();
	}

	/// <summary> Passed to <see cref="ChooseAsync"/> to end a Fast or Slow phase without resolving anything. </summary>
	public const int Done = -1;

	void EndFast() {
		SetPhase( Phase.Invaders );
		_step = RoundStep.Invaders;
	}

	/// <summary>
	/// A decision timed out mid-step, so the game is in a half-finished state that can't be saved. Discards it
	/// by reloading the last checkpoint and returns that checkpoint to be written to disk. Resume with
	/// Load(checkpoint) then AdvanceAsync(): the step that was prompting restarts from its beginning.
	/// </summary>
	PendingMenu Suspend() {
		Load( _checkpoint! );
		Suspended = _checkpoint;
		return new PendingMenu( MenuKind.Suspended, [], _checkpoint );
	}

	/// <summary> Set when the game gave up waiting on the user; the save to resume from. </summary>
	public JsonObject? Suspended { get; private set; }

	JsonObject? _checkpoint;

	/// <summary>
	/// Counts the checkpoints that moved the game forward (a menu, or between Invader steps). Unlike
	/// <see cref="LatestCheckpoint"/>'s identity it does not change when <see cref="ChooseAsync"/> re-saves the
	/// state of the menu it is about to resolve, so a driver can tell which of its picks come after the checkpoint.
	/// </summary>
	public int CheckpointVersion { get; private set; }

	void NewCheckpoint() {
		_checkpoint = Save();
		++CheckpointVersion;
	}

	/// <summary>
	/// The save as of the most recent action boundary (a menu, or between Invader steps) - always safe to
	/// write to disk, even while the engine is partway through an action waiting on the user (unlike
	/// <see cref="Save"/>, which is only valid at a boundary). Saving it loses just the current action's
	/// unfinished decisions. Null until the first menu. Safe to read from another thread: a checkpoint is
	/// never modified after it is created.
	/// </summary>
	public JsonObject? LatestCheckpoint => _checkpoint;

	/// <summary> A menu is a clean action boundary, so it is also the newest checkpoint. </summary>
	PendingMenu Menu( MenuKind kind, IActionFactory[] options ) {
		NewCheckpoint();
		return new PendingMenu( kind, options );
	}

	PendingMenu GameOver( GameOverException gameOver ) {
		GameState.Result = gameOver.Status;
		GameState.Log( gameOver.Status );
		return new PendingMenu( MenuKind.GameOver, [] );
	}

	/// <summary>
	/// Asks the user, through Spirit's decision gateway, to pick from the menu. This is what keeps the
	/// long-lived StartAsync loop working with the existing prompts; a suspend/resume driver would skip
	/// this and pass the user's pick straight to <see cref="ChooseAsync"/>.
	/// </summary>
	async Task<int> UserPicksAsync( PendingMenu menu ) {
		switch( menu.Kind ) {
			case MenuKind.PlayCards:
				return 0;
			case MenuKind.Growth:
				IActionFactory growth = await Spirit.SelectAlways( new A.GrowthDecision( "Select Growth", menu.Options, Present.Always ) );
				return Array.IndexOf( menu.Options, growth );
			default:
				Phase phase = menu.Kind switch { MenuKind.Init => Phase.Init, MenuKind.Fast => Phase.Fast, _ => Phase.Slow };
				Present present = phase == Phase.Init ? Present.Always : Present.Done;
				IActionFactory? option = await Spirit.Select( new A.TypedDecision<IActionFactory>( "Select " + phase + " to resolve", menu.Options, present ) );
				return option is null ? Done : Array.IndexOf( menu.Options, option );
		}
	}

	#endregion Stepwise core

	#region Save / Load

	/// <summary>
	/// The whole game as of now: GameState plus the round-step cursor, so it can be written to disk and
	/// later passed to <see cref="Load"/> on a freshly built SoloGame. Only valid at an action boundary -
	/// not while an action is awaiting a decision (see docs/GameSerialization-Roadmap.md).
	/// </summary>
	public JsonObject Save() {
		JsonObject snapshot = GameState.ToJson( new GameStateSerializationContext( GameState ) );
		snapshot["RoundStep"] = (int)_step;
		snapshot["InvaderCursor"] = _invader.ToJson();
		return snapshot;
	}

	/// <summary>
	/// Restores a <see cref="Save"/> onto this game. Precondition: this SoloGame was built the same way as
	/// the one that was saved (see GameState.RestoreFromJson).
	/// </summary>
	public void Load( JsonObject snapshot ) {
		_step = (RoundStep)snapshot["RoundStep"]!.GetValue<int>();
		if( snapshot["InvaderCursor"] is JsonArray cursor ) _invader.RestoreFromJson( cursor );
		else _invader.Reset();
		GameState.RestoreFromJson( snapshot, new GameStateSerializationContext( GameState ) );
	}

	#endregion Save / Load

	void SetPhase(Phase phase ) {
		GameState.Phase = phase;
		GameState.Log( new Log.Phase( GameState.Phase ) );
	}

	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );

	readonly List<JsonObject> _stateStack = [];

}

public enum MenuKind { Init, Growth, PlayCards, Fast, Slow, GameOver, Suspended }

/// <summary>
/// What the game is waiting on between actions. Derived from game state, so it is never saved - it is
/// recomputed by <see cref="SoloGame.AdvanceAsync"/> after a Load.
/// </summary>
/// <param name="Options">The actions to pick from; empty for PlayCards (one grouped action), GameOver and Suspended.</param>
/// <param name="Checkpoint">Only for Suspended: the save to write to disk and later Load.</param>
public record PendingMenu( MenuKind Kind, IActionFactory[] Options, JsonObject? Checkpoint = null );
