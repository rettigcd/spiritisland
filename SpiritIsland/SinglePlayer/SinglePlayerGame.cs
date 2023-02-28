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
		this.GameState = gameState;
		Spirit = gameState.Spirits.Single(); // this player only handles single-player.
		this.UserPortal = Spirit.Gateway;
	}


	#endregion

	public SinglePlayerGame Start() {

		async Task LoopAsync() {
			ActionScope.Initialize();
			UserGateway.UsePreselect.Value = EnablePreselects;
			try {
				// Handle any unresolved Initialization action - (ocean/beast)
				GameState.Phase = Phase.Init;
				await Spirit.ResolveActions( GameState ); 

				Dictionary<int,IMemento<GameState>> savedGameStates = new Dictionary<int, IMemento<GameState>>();
				while(true) {
					savedGameStates[GameState.RoundNumber] = GameState.SaveToMemento();
					DateTime lastSaveTimeStamp= DateTime.Now;
					try {
						LogRound();

						GameState.Phase = Phase.Growth;
						LogPhase();
						await Spirit.DoGrowth( GameState ); // !
						await Spirit.SelectAndPlayCardsFromHand();

						GameState.Phase = Phase.Fast;
						LogPhase();
						await Spirit.ResolveActions( GameState );

						GameState.Phase = Phase.Invaders;
						LogPhase();
						await InvaderPhase.ActAsync( GameState );

						GameState.Phase = Phase.Slow;
						LogPhase();
						await Spirit.ResolveActions( GameState );

						await GameState.TriggerTimePasses();
					} catch( GameStateCommandException cmdEx ) {
						if(cmdEx.Cmd is Rewind rewind && savedGameStates.ContainsKey(rewind.TargetRound)) {
							GameState.LoadFrom( savedGameStates[rewind.TargetRound] );
							foreach(int laterRounds in savedGameStates.Keys.Where(k=>k>rewind.TargetRound).ToArray())
								savedGameStates.Remove(laterRounds);
						}
					} catch(Exception ex) when (ex is not GameOverException) {
						GameState.Log( new Log.Exception( ex ) );
						GameState.LoadFrom( savedGameStates[ GameState.RoundNumber ] ); // go back to beginning of round and see if we can debug it.
					}
				}
			}
			catch(GameOverException gameOver) {
				this.GameState.Result = gameOver.Status;
				GameState.Log( gameOver.Status );
			}
			catch(Exception ex) {
				GameState.Log(new Log.Exception( ex ) );
			}
		}
		_ = LoopAsync();

		return this;
	}

	void LogPhase() => GameState.Log( new Log.Phase( GameState.Phase ) );
	void LogRound() => GameState.Log( new Log.Round( GameState.RoundNumber ) );
 		
}