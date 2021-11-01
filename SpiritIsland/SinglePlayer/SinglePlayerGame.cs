﻿using System;
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

		public SinglePlayerGame(GameState gameState){
			this.GameState = gameState;
			gameState.Initialize();
			Spirit = gameState.Spirits.Single(); // this player only handles single-player.
			this.UserPortal = Spirit.Action;
			StartLoop();
		}


		#endregion

		#region private

		void StartLoop() {

			var fastActions = new ResolveActions( Spirit, GameState, Phase.Fast, true );
			var slowActions = new ResolveActions( Spirit, GameState, Phase.Slow, true );

			async Task LoopAsync() {
				try {
					// Handle any unresolved Initialization action - (ocean/beast)
					await new ResolveActions( Spirit, GameState, Phase.Growth, false ).ActAsync();

					Stack<IMemento<GameState>> savedGameStates = new Stack<IMemento<GameState>>();
					while(true) {
						savedGameStates.Push( GameState.SaveToMemento() );
						DateTime lastSaveTimeStamp= DateTime.Now;
						try {
							GameState.Phase = Phase.Growth;
							await Spirit.DoGrowth(GameState);
							await Spirit.PurchasePlayableCards();

							GameState.Phase = Phase.Fast;
							await fastActions.ActAsync();

							GameState.Phase = Phase.Invaders;
							await GameState.InvaderEngine.DoInvaderPhase();

							GameState.Phase = Phase.Slow;
							await slowActions.ActAsync();

							await GameState.TimePasses();
						} catch( GameStateCommandException ) {
							// if they want to go back withing 5 seconds of a save, throw away the save and go back one more slot
							if(DateTime.Now < lastSaveTimeStamp.Add(TimeSpan.FromSeconds(5)) && savedGameStates.Count>1)
								savedGameStates.Pop();
							GameState.LoadFrom( savedGameStates.Pop() );
						}
					}
				}
				catch(GameOverException gameOver) {
					this.WinLoseStatus = gameOver.Status;
				}
				catch(Exception ex) {
					if(LogExceptions) {
						const string filename = @"C:\users\rettigcd\desktop\si_log.txt";
						System.IO.File.AppendAllText(filename, $"\r\n===={DateTime.Now}=====\r\n"+ex.ToString() );
						await Spirit.UserSelectsFirstText("Exception - Check Log File!", filename, "program is going to lock up");
					} else {
						await Spirit.UserSelectsFirstText( "Exception!", "-", "-" );
						// !!! need to detect if all presence is destroyed
					}
				}
			}
			_ = LoopAsync();

		}

		#endregion

	}


}
