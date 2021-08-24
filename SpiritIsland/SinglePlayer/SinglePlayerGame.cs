using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SinglePlayerGame {

		/// <summary> The main interface that drives the UI</summary>
		public IDecisionStream DecisionProvider {get; set;}

		// Public for querying gamestate ad hoc
		public GameState GameState { get; }

		// simplies accessing the only spirit playing
		public Spirit Spirit {get; set;}

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
            var resolveGrowth = new ResolveActions( Spirit, GameState, Speed.Growth );
            var selectPowerCards = new SelectPowerCards( Spirit );
            var fastActions = new ResolveActions( Spirit, GameState, Speed.Fast, true );
            var invaders = new InvaderPhase( GameState );
            var slowActions = new ResolveActions( Spirit, GameState, Speed.Slow, true );
            var timePasses = new TimePasses( GameState );

			invaders.NewLogEntry += OnNewLogEntry;

			selectGrowth.Complete     += () => resolveGrowth.Initialize();
			resolveGrowth.Complete    += () => selectPowerCards.Initialize();
			selectPowerCards.Complete += () => fastActions.Initialize();
			fastActions.Complete      += () => invaders.Initialize();
			invaders.Complete         += () => slowActions.Initialize();
			slowActions.Complete      += () => timePasses.Initialize();
			timePasses.Complete       += () => selectGrowth.Initialize();

			selectGrowth.Initialize();

			//async Task Loop() {
			//	while(true) {
			//		await selectGrowth.ActAsync();
			//		await resolveGrowth.ActAsync();
			//		await selectPowerCards.ActAsync();
			//		await fastActions.ActAsync();
			//		await invaders.ActAsync();
			//		await slowActions.ActAsync();
			//		await timePasses.ActAsync();
			//	}
			//}
			//_ = Loop();


		}

		#endregion

	}


}
