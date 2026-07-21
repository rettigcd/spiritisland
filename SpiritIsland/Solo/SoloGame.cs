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

	public async Task StartAsync() {

		UserGateway.UsePreselect.Value = EnablePreselects;
		try {
			// Handle any unresolved Initialization action - (ocean/beast)
			GameState.Phase = Phase.Init;
			while( await Spirit.SelectAndResolveNextAction( GameState ) ) { }

			while(true) {
				SaveGameState();
				try {
					while( await Do1Action() ) { }
				}
				catch( RewindException rewind ) {
					RewindGameTo( rewind );
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

	void SaveGameState() {
		_savedGameStates[GameState.RoundNumber] = ((IHaveMemento)GameState).Memento;
	}

	void RewindGameTo( RewindException rex ) {
		if( !_savedGameStates.TryGetValue(rex.TargetRound, out object? memento) ) return;

		// Restore
		((IHaveMemento)GameState).Memento = memento;

		// Clear later rounds
		foreach( int laterRounds in _savedGameStates.Keys.Where(k => rex.TargetRound < k ).ToArray())
			_savedGameStates.Remove(laterRounds);

		// Do this last so anything that anything that triggers off of this, has new games state.
		GameState.Log(rex);
	}

	enum RoundStep { RoundStart, Growth, EndGrowth, PlayCards, Fast, Invaders, Slow, TimePasses }

	RoundStep _step = RoundStep.RoundStart;

	/// <summary>
	/// Advances the round by exactly one action - safe to call repeatedly in a loop. Growth/Fast/Slow
	/// resolve one decision per call each (via Spirit's own granular step methods); RoundStart/EndGrowth/
	/// PlayCards/Invaders/TimePasses are each a single coarse step since Spirit/InvaderPhase don't (yet)
	/// expose anything more granular for them. Returns false exactly once, when TimePasses completes and
	/// the round is done; true otherwise.
	/// </summary>
	public async Task<bool> Do1Action() {
		switch( _step ) {

			case RoundStep.RoundStart:
				LogRound();
				SetPhase( Phase.Growth );
				Spirit.GrowthTrack.Reset();
				_step = RoundStep.Growth;
				return true;

			case RoundStep.Growth:
				if( Spirit.HasMoreGrowthActions )
					await Spirit.SelectAndResolveNextGrowthAction();
				else
					_step = RoundStep.EndGrowth;
				return true;

			case RoundStep.EndGrowth:
				await Spirit.EndGrowth();
				_step = RoundStep.PlayCards;
				return true;

			case RoundStep.PlayCards:
				// not really an action at all - no need to split it up.
				await Spirit.SelectAndPlayCardsFromHand();
				SetPhase( Phase.Fast );
				_step = RoundStep.Fast;
				return true;

			case RoundStep.Fast:
				if( !await Spirit.SelectAndResolveNextAction(GameState) ) {
					SetPhase(Phase.Invaders);
					_step = RoundStep.Invaders;
				}
				return true;

			case RoundStep.Invaders:
				await InvaderPhase.ActAsync( GameState );
				SetPhase( Phase.Slow );
				_step = RoundStep.Slow;
				return true;

			case RoundStep.Slow:
				if( !await Spirit.SelectAndResolveNextAction(GameState) )
					_step = RoundStep.TimePasses;

				return true;

			case RoundStep.TimePasses:
				await GameState.TriggerTimePasses();
				_step = RoundStep.RoundStart;
				return false;

			default:
				throw new InvalidOperationException( $"Unhandled RoundStep {_step}" );
		}
	}

	void SetPhase(Phase phase ) {
		GameState.Phase = phase;
		GameState.Log( new Log.Phase( GameState.Phase ) );
	}

	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );

	readonly Dictionary<int, object> _savedGameStates = [];

}
