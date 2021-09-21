using System;
using System.Linq;
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
			Log( "Ravaging:" + gameState.InvaderDeck.Ravage.Select(x=>x.Text).Join("/") + "\r\n" + ravageResults.Join( "\r\n" ) );

			// Building
			Log( "Building:" + gameState.InvaderDeck.Build.Select(x=>x.Text).Join("/") );
			var builds = await gameState.Build();
			Log( builds.Join( "\r\n" ) );

			// Exploring
			Log( "Exploring:" + (invaderDeck.Explore.Count > 0 ? invaderDeck.Explore[0].Text : "-") );
			await gameState.Explore( invaderDeck.Explore[0] );

			invaderDeck.Advance();
		}

		void Log( string msg ) => NewLogEntry?.Invoke( msg );

		#region private fields

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		#endregion




	}

}
