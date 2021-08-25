﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class InvaderPhase {

		public InvaderPhase(GameState gameState){
			this.gameState = gameState;
			this.spirit = gameState.Spirits[0];
			this.invaderDeck = gameState.InvaderDeck;
		}

		public event Action<string> NewLogEntry;

		public async Task ActAsync() {

			var decisionMaker = spirit;

			// Blight
			if(gameState.BlightCard.IslandIsBlighted) {
				Log( "Island is blighted" );
				await gameState.BlightCard.OnStartOfInvaders( gameState );
			}

			// Fear
			await gameState.ApplyFear();

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
			var builds = await gameState.Build( invaderDeck.Build );
			Log( builds.Join( "\r\n" ) );

			// Exploring
			Log( "Exploring:" + invaderDeck.Explore?.Text ?? "-" );
			gameState.Explore( invaderDeck.Explore );

			invaderDeck.Advance();
		}

		void Log( string msg ) => NewLogEntry?.Invoke( msg );

		#region private fields

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		#endregion




	}

}
