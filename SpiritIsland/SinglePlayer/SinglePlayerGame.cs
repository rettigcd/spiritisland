using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SinglePlayerGame {

		public WinLoseStatus WinLoseStatus = WinLoseStatus.Playing;

		/// <summary> The main interface that drives the UI</summary>
		public IDecisionStream DecisionProvider {get; set;}

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
			this.DecisionProvider = Spirit.Action;
			InitPhases();
		}


		#endregion

		public event Action<string> NewLogEntry;

		#region private

		void OnNewLogEntry( string msg ) {
			NewLogEntry?.Invoke( msg );
		}

		void InitPhases() {

            var selectGrowth = new SelectGrowth( Spirit, GameState );
            var resolveGrowth = new ResolveActions( Spirit, GameState, Speed.Growth, false );
            var fastActions = new ResolveActions( Spirit, GameState, Speed.Fast, true );
            var invaders = new InvaderPhase( GameState );
            var slowActions = new ResolveActions( Spirit, GameState, Speed.Slow, true );

			invaders.NewLogEntry += OnNewLogEntry;

			async Task LoopAsync() {
				try {
					while(true) {
						await selectGrowth.ActAsync();
						await resolveGrowth.ActAsync();
						await Spirit.BuyPowerCardsAsync();
						await fastActions.ActAsync();
						await invaders.ActAsync();
						await slowActions.ActAsync();
						await GameState.TimePasses();
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
					}
				}
			}
			_ = LoopAsync();

		}

		#endregion

	}


}
