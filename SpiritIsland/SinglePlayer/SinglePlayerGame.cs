namespace SpiritIsland.SinglePlayer;

public class SinglePlayerGame {

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

	public SinglePlayerGame(GameState gameState){
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
			await Spirit.SelectAndResolveActions( GameState ); 

			while(true) {
				SaveGameState();
				try {
					await Do1Round();
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

	async Task Do1Round() {
		LogRound();

		SetPhase( Phase.Growth );
		await Spirit.DoGrowth( GameState ); // !
		await Spirit.SelectAndPlayCardsFromHand();
		
		SetPhase( Phase.Fast );
		await Spirit.SelectAndResolveActions( GameState );

		SetPhase( Phase.Invaders );
		await InvaderPhase.ActAsync( GameState );

		SetPhase( Phase.Slow );
		await Spirit.SelectAndResolveActions( GameState );

		await GameState.TriggerTimePasses();
	}

	void SetPhase(Phase phase ) {
		GameState.Phase = phase;
		GameState.Log( new Log.Phase( GameState.Phase ) );
	}

	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );

	readonly Dictionary<int, object> _savedGameStates = [];

}
