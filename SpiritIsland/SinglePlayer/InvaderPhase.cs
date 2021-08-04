using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class InvaderPhase : IPhase {

		public InvaderPhase(GameState gameState){
			this.gameState = gameState;
			this.spirit = gameState.Spirits[0];
			this.invaderDeck = gameState.InvaderDeck;
		}

		readonly Spirit spirit;

		public string Prompt => spirit.decisions.Peek().Prompt;
		public IOption[] Options => spirit.decisions.Peek().Options;
		public void Select( IOption option ) {
			var decision = spirit.decisions.Pop();
			decision.Select( option );
		}


		public event Action<string> NewLogEntry;
		public event Action Complete;

		public void Initialize() {
			engine = new ActionEngine( gameState.Spirits[0], gameState );
			_ = Action();
		}


		async Task Action() {
			// Blight
			if(gameState.BlightCard.IslandIsBlighted) {
				Log( "Island is blighted" );
				await gameState.BlightCard.OnStartOfInvaders( gameState );
			}

			// Fear
			Log( $"Fear Pool:{gameState.FearPool} Activated:{gameState.ActivatedFearCards.Count}" );
			while(gameState.ActivatedFearCards.Count > 0) {
				var card = gameState.ActivatedFearCards.Pop();
				switch(gameState.TerrorLevel) {
					case 1: await card.Level1( gameState ); break;
					case 2: await card.Level2( gameState ); break;
					case 3: await card.Level3( gameState ); break;
				}
				Log( $"Applying Fear Card" );
			}
			//			await gameState.ApplyFear();

			// Ravage
			string[] ravageResults = gameState.Ravage( invaderDeck.Ravage );
			Log( "Ravaging:" + (invaderDeck.Ravage?.Text ?? "-") + "\r\n" + ravageResults.Join( "\r\n" ) );

			// Cascade blight
			while(gameState.cascadingBlight.Count > 0) {
				Space blightedSpace = gameState.cascadingBlight.Pop();
				Space cascadeSpace = await engine.SelectSpace( "Select land to cascade blight from " + blightedSpace.Label,
					blightedSpace.Neighbors
						.Where( x => x.Terrain != Terrain.Ocean )
				);
				gameState.AddBlight( cascadeSpace );
			}

			// Building
			Log( "Building:" + invaderDeck.Build?.Text ?? "-" );
			var builds = gameState.Build( invaderDeck.Build );
			Log( builds.Join( "\r\n" ) );

			// Exploring
			Log( "Exploring:" + invaderDeck.Explore?.Text ?? "-" );
			gameState.Explore( invaderDeck.Explore );

			invaderDeck.Advance();

			this.Complete?.Invoke();
		}

		ActionEngine engine;

		void Log( string msg ) => NewLogEntry?.Invoke( msg );

		#region private fields

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		#endregion




	}

}
