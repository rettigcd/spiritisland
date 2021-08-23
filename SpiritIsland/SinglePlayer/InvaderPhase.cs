using System;
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

		public event Action<string> NewLogEntry;
		public event Action Complete;

		public void Initialize() {
			_ = ActAndTrigger();
		}

		public async Task ActAndTrigger() {
			await ActAsync();
			this.Complete?.Invoke();
		}


		public async Task ActAsync() {

			var decisionMaker = spirit;

			// Blight
			if(gameState.BlightCard.IslandIsBlighted) {
				Log( "Island is blighted" );
				await gameState.BlightCard.OnStartOfInvaders( gameState );
			}

			// Fear
			Log( $"Fear Pool:{gameState.FearPool} Activated:{gameState.ActivatedFearCards.Count}" );
			while(gameState.ActivatedFearCards.Count > 0) {
				var card = gameState.ActivatedFearCards.Pop().Card;
				switch(gameState.TerrorLevel) {
					case 1: await card.Level1( gameState ); break;
					case 2: await card.Level2( gameState ); break;
					case 3: await card.Level3( gameState ); break;
				}
				Log( $"Applying Fear Card" );
			}

			// Ravage
			string[] ravageResults = await gameState.Ravage( invaderDeck.Ravage );
			Log( "Ravaging:" + (invaderDeck.Ravage?.Text ?? "-") + "\r\n" + ravageResults.Join( "\r\n" ) );

			// Cascade blight
			while(gameState.cascadingBlight.Count > 0) {
				Space blightedSpace = gameState.cascadingBlight.Pop();
				Space cascadeSpace = await decisionMaker.SelectSpace( "Select land to cascade blight from " + blightedSpace.Label,
					blightedSpace.Adjacent
						.Where( x => x.Terrain != Terrain.Ocean ) // $OCEAN$ - blight
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
		}

		void Log( string msg ) => NewLogEntry?.Invoke( msg );

		#region private fields

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		#endregion




	}

}
