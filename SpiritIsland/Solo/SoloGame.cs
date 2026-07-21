namespace SpiritIsland.SinglePlayer;

public class SoloGame {

	/// <summary> The main interface that drives the UI</summary>
	public IGamePortal UserPortal {get; set;}

	// Public for querying gamestate ad hoc
	public GameState GameState { get; }

	// simplies accessing the only spirit playing
	public Spirit Spirit {get; set;}

	public bool LogExceptions { get; set; }

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

	public async Task StartAsync() {

		UserGateway.UsePreselect.Value = EnablePreselects;
		try {
			// Handle any unresolved Initialization action - (ocean/beast)
			GameState.Phase = Phase.Init;
			while( await Spirit.SelectAndResolveNextAction( GameState ) ) { }

			while(true) {
				try {
					await Do1Action();
				}
				catch( RewindException rewind ) {
					RewindOneAction( rewind );
				}
			}
		}
		catch(GameOverException gameOver) {
			GameState.Result = gameOver.Status;
			GameState.Log( gameOver.Status );
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
	/// Do1Action() call retries fresh from there. If only one snapshot exists (the very first action of
	/// the game), the first discard is skipped so there's still something to restore from - it's reload
	/// (not discard) that makes it a no-op, not leaving the entry behind afterward.
	/// </summary>
	void RewindOneAction( RewindException rex ) {
		if( 1 < _stateStack.Count )
			_stateStack.RemoveAt( _stateStack.Count - 1 );

		JsonObject snapshot = _stateStack[^1];
		_step = (RoundStep)snapshot["RoundStep"]!.GetValue<int>();
		GameState.RestoreFromJson( snapshot, new GameStateSerializationContext( GameState ) );

		_stateStack.RemoveAt( _stateStack.Count - 1 );

		// Do this last so anything that triggers off of this, has new game state.
		GameState.Log(rex);
	}

	enum RoundStep { RoundStart, Growth, EndGrowth, PlayCards, Fast, Invaders, Slow, TimePasses }

	RoundStep _step = RoundStep.RoundStart;

	/// <summary>
	/// Advances the round by exactly one action - safe to call repeatedly in a loop. Growth/Fast/Slow
	/// resolve one decision per call each (via Spirit's own granular step methods); PlayCards is a single
	/// coarse step since Spirit doesn't (yet) expose anything more granular for it. Only steps that can
	/// actually show the user something call PushGameState() - RoundStart/EndGrowth/Invaders/TimePasses
	/// never do (nothing to undo back to), and Fast/Slow additionally discard their own snapshot when
	/// Spirit.HadNextActionOptions comes back false, so a rewind always lands on the state as of the
	/// user's last real decision, however many silent steps ran since. Returns false exactly once, when
	/// TimePasses completes and the round is done; true otherwise.
	/// </summary>
	public async Task<bool> Do1Action() {
		switch( _step ) {

			case RoundStep.RoundStart:
				// Never has anything for the user to undo back to - no PushGameState().
				LogRound();
				SetPhase( Phase.Growth );
				Spirit.GrowthTrack.Reset();
				_step = RoundStep.Growth;
				return true;

			case RoundStep.Growth:
				PushGameState();
				if( Spirit.HasMoreGrowthActions )
					await Spirit.SelectAndResolveNextGrowthAction();
				else {
					PopGameState(); // nothing was presented - not an undo-able step
					_step = RoundStep.EndGrowth;
				}
				return true;

			case RoundStep.EndGrowth:
				// Never has anything for the user to undo back to - no PushGameState().
				await Spirit.EndGrowth();
				_step = RoundStep.PlayCards;
				return true;

			case RoundStep.PlayCards:
				// not really an action at all - no need to split it up.
				PushGameState();
				await Spirit.SelectAndPlayCardsFromHand();
				SetPhase( Phase.Fast );
				_step = RoundStep.Fast;
				return true;

			case RoundStep.Fast:
				PushGameState();
				if( !await Spirit.SelectAndResolveNextAction(GameState) ) {
					if( !Spirit.HadNextActionOptions )
						PopGameState(); // nothing was presented - not an undo-able step
					SetPhase(Phase.Invaders);
					_step = RoundStep.Invaders;
				}
				return true;

			case RoundStep.Invaders:
				// Never has anything for the user to undo back to - no PushGameState().
				await InvaderPhase.ActAsync( GameState );
				SetPhase( Phase.Slow );
				_step = RoundStep.Slow;
				return true;

			case RoundStep.Slow:
				PushGameState();
				if( !await Spirit.SelectAndResolveNextAction(GameState) ) {
					if( !Spirit.HadNextActionOptions )
						PopGameState(); // nothing was presented - not an undo-able step
					_step = RoundStep.TimePasses;
				}

				return true;

			case RoundStep.TimePasses:
				// Never has anything for the user to undo back to - no PushGameState().
				await GameState.TriggerTimePasses();
				_step = RoundStep.RoundStart;
				return false;

			default:
				throw new InvalidOperationException( $"Unhandled RoundStep {_step}" );
		}
	}

	void PushGameState() {
		JsonObject snapshot = GameState.ToJson( new GameStateSerializationContext( GameState ) );
		snapshot["RoundStep"] = (int)_step;
		_stateStack.Add( snapshot );
	}

	void PopGameState() => _stateStack.RemoveAt( _stateStack.Count - 1 );

	void SetPhase(Phase phase ) {
		GameState.Phase = phase;
		GameState.Log( new Log.Phase( GameState.Phase ) );
	}

	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );

	readonly List<JsonObject> _stateStack = [];

}
