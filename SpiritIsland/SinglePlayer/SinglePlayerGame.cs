using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SinglePlayerGame {

		public WinLoseStatus WinLoseStatus = WinLoseStatus.Playing;

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
					await Spirit.ResolveActions( new SpiritGameStateCtx( Spirit, GameState, Cause.Growth ) ); // !!! if this is here, why do we need to put it in the Spirit.Growth() method?

					Stack<IMemento<GameState>> savedGameStates = new Stack<IMemento<GameState>>();
					while(true) {
						savedGameStates.Push( GameState.SaveToMemento() );
						DateTime lastSaveTimeStamp= DateTime.Now;
						try {
							LogRound();

							GameState.Phase = Phase.Growth;
							LogPhase();
							await Spirit.DoGrowth( GameState );
							await Spirit.PlayCardsFromHand();

							GameState.Phase = Phase.Fast;
							LogPhase();
							await Spirit.ResolveActions( new SpiritGameStateCtx( Spirit, GameState, Cause.Power ) );

							GameState.Phase = Phase.Invaders;
							LogPhase();
							await GameState.InvaderEngine.DoInvaderPhase();

							GameState.Phase = Phase.Slow;
							LogPhase();
							await Spirit.ResolveActions( new SpiritGameStateCtx( Spirit, GameState, Cause.Power ) );

							await GameState.TriggerTimePasses();
						} catch( GameStateCommandException ) {
							// if they want to go back withing 5 seconds of a save, throw away the save and go back one more slot
							if(DateTime.Now < lastSaveTimeStamp.Add(TimeSpan.FromSeconds(5)) && savedGameStates.Count>1)
								savedGameStates.Pop();
							GameState.LoadFrom( savedGameStates.Pop() );
						}
					}
				}
				catch(GameOverException gameOver) {
					this.WinLoseStatus = gameOver.Status; // Put this on GameState
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
