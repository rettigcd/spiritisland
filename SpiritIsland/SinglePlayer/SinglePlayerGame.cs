﻿using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIsland.SinglePlayer {

	public class SinglePlayerGame {

		/// <summary> The main interface that drives the UI</summary>
		public IDecision Decision {get; set;}

		// Public for querying gamestate ad hoc
		public GameState GameState { get; }

		// simplies accessing the only spirit playing
		public Spirit Spirit {get; set;}

		#region constructor 

		public SinglePlayerGame(GameState gameState){
			this.GameState = gameState;
			gameState.InitIsland();
			gameState.InitBlight(BlightCard.DownwardSpiral);
			Spirit = gameState.Spirits.Single(); // this player only handles single-player.
			InitPhases();
		}

		#endregion

		#region private

		void InitPhases() {
			var selectGrowth = new SelectGrowth( Spirit, GameState );
			var resolveGrowth = new ResolveActions( Spirit, GameState, Speed.Growth );
			var selectPowerCards = new SelectPowerCards( Spirit );
			var fastActions = new ResolveActions( Spirit, GameState, Speed.Fast, true );
			var invaders = new InvaderPhase( GameState );
			var slowActions = new ResolveActions( Spirit, GameState, Speed.Slow, true );
			var timePasses = new TimePasses( Spirit, GameState );

			selectGrowth.Complete += () => TransitionTo( resolveGrowth );
			resolveGrowth.Complete += () => TransitionTo( selectPowerCards );
			selectPowerCards.Complete += () => TransitionTo( fastActions );
			fastActions.Complete += () => TransitionTo( invaders );
			invaders.Complete += () => TransitionTo( slowActions );
			slowActions.Complete += () => TransitionTo( timePasses );
			timePasses.Complete += () => TransitionTo( selectGrowth );

			TransitionTo( selectGrowth );
		}

		void TransitionTo(IPhase phase){
			this.Decision = phase; // ! this must go first! because .Initialize might trigger the next phase
			phase.Initialize();
		}

		#endregion

	}


}
