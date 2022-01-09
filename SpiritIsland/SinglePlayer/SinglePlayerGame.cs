using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SinglePlayerGame {

		/// <summary> The main interface that drives the UI</summary>
		public IUserPortal UserPortal {get; set;}

		// Public for querying gamestate ad hoc
		public GameState GameState { get; }

		// simplies accessing the only spirit playing
		public Spirit Spirit {get; set;}

		public bool LogExceptions { get; set; }

		#region constructor 

		public SinglePlayerGame(GameState gameState,bool start=true){
			this.GameState = gameState;
			gameState.Initialize();
			Spirit = gameState.Spirits.Single(); // this player only handles single-player.
			this.UserPortal = Spirit.Action;
			if(start)
				Start();
		}


		#endregion

		public void Start() {

			async Task LoopAsync() {
				try {
					// Handle any unresolved Initialization action - (ocean/beast)
					GameState.Phase = Phase.Init;
					await Spirit.ResolveActions( new SelfCtx( Spirit, GameState, Cause.Growth ) ); 
					// !!! if this is here, why do we need to put it in the Spirit.Growth() method?

					Dictionary<int,IMemento<GameState>> savedGameStates = new Dictionary<int, IMemento<GameState>>();
					while(true) {
						savedGameStates[GameState.RoundNumber] = GameState.SaveToMemento();
						DateTime lastSaveTimeStamp= DateTime.Now;
						try {
							LogRound();

							GameState.Phase = Phase.Growth;
							LogPhase();
							await Spirit.DoGrowth( GameState );
							await Spirit.SelectAndPlayCardsFromHand();

							GameState.Phase = Phase.Fast;
							LogPhase();
							await Spirit.ResolveActions( new SelfCtx( Spirit, GameState, Cause.Power ) );

							GameState.Phase = Phase.Invaders;
							LogPhase();
							await InvaderPhase.ActAsync( GameState );

							GameState.Phase = Phase.Slow;
							LogPhase();
							await Spirit.ResolveActions( new SelfCtx( Spirit, GameState, Cause.Power ) );

							await GameState.TriggerTimePasses();
						} catch( GameStateCommandException cmdEx ) {
							if(cmdEx.Cmd is Rewind rewind && savedGameStates.ContainsKey(rewind.TargetRound)) {
								GameState.LoadFrom( savedGameStates[rewind.TargetRound] );
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
					GameState.Log(new LogException( ex ) );
				}
			}
			_ = LoopAsync();

		}

		void LogPhase() => GameState.Log( new LogPhase( GameState.Phase ) );
		void LogRound() => GameState.Log( new LogRound( GameState.RoundNumber ) );
 		
	}

}
