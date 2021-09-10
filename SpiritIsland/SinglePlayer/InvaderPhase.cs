using System;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class InvaderPhase {

		public InvaderPhase(GameState gameState){
			this.gameState = gameState;
			this.invaderDeck = gameState.InvaderDeck;
		}

		public event Action<string> NewLogEntry;

		public async Task ActAsync() {

			// Blight
			if(gameState.BlightCard.IslandIsBlighted) {
				Log( "Island is blighted" );
				await gameState.BlightCard.OnStartOfInvaders( gameState );
			}

			// Fear
			await gameState.Fear.Apply();

			// Ravage
			string[] ravageResults = await gameState.Ravage();
			Log( "Ravaging:" + (invaderDeck.Ravage?.Text ?? "-") + "\r\n" + ravageResults.Join( "\r\n" ) );

			// Building
			Log( "Building:" + invaderDeck.Build?.Text ?? "-" );
			var builds = await gameState.Build();
			Log( builds.Join( "\r\n" ) );

			// Exploring
			Log( "Exploring:" + invaderDeck.Explore?.Text ?? "-" );
			await gameState.Explore( invaderDeck.Explore );

			invaderDeck.Advance();
		}

		void Log( string msg ) => NewLogEntry?.Invoke( msg );

		#region private fields

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		#endregion




	}

}
