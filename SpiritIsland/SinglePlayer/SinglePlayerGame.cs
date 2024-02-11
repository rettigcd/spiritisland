namespace SpiritIsland.SinglePlayer;

public class SinglePlayerGame {

	/// <summary> The main interface that drives the UI</summary>
	public IUserPortal UserPortal {get; set;}

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
		UserPortal = Spirit.Portal;
	}


	#endregion

	/// <summary>
	/// Starts game engine (asynchronously) and saves the Task to .EngineTask
	/// </summary>
	public void Start() {
		// this MUST be called at least once on the UI thread so that we can query the GameState
		ActionScope.Initialize( GameState );
		EngineTask = StartAsync();
	}
	public Task EngineTask { get; private set; }

	public async Task StartAsync() {

		UserGateway.UsePreselect.Value = EnablePreselects;
		try {
			// Handle any unresolved Initialization action - (ocean/beast)
			GameState.Phase = Phase.Init;
			await Spirit.ResolveActions( GameState ); 

			Dictionary<int,object> savedGameStates = [];
			while(true) {
				savedGameStates[GameState.RoundNumber] = ((IHaveMemento)GameState).Memento;
				DateTime lastSaveTimeStamp= DateTime.Now;
				try {
					await Do1Round();
				}
				catch( RewindException rewind ) {
					if(savedGameStates.TryGetValue( rewind.TargetRound, out object memento )) {
						((IHaveMemento)GameState).Memento = memento;
						foreach(int laterRounds in savedGameStates.Keys.Where(k=>k>rewind.TargetRound).ToArray())
							savedGameStates.Remove(laterRounds);
					}
				}
			}
		}
		catch(GameOverException gameOver) {
			this.GameState.Result = gameOver.Status;
			GameState.Log( gameOver.Status );
		}
		catch(Exception ex) {
			GameState.Log(new Log.ExceptionEntry( ex ) );
		}

	}

	async Task Do1Round() {
		LogRound();

		SetPhase( Phase.Growth );
		await Spirit.DoGrowth( GameState ); // !
		await Spirit.SelectAndPlayCardsFromHand();
		
		SetPhase( Phase.Fast );
		await Spirit.ResolveActions( GameState );

		SetPhase( Phase.Invaders );
		await InvaderPhase.ActAsync( GameState );

		SetPhase( Phase.Slow );
		await Spirit.ResolveActions( GameState );

		await GameState.TriggerTimePasses();
	}

	void SetPhase(Phase phase ) {
		GameState.Phase = phase;
		GameState.Log( new Log.Phase( GameState.Phase ) );
	}

	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );
 		
}