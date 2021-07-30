using System;
using System.Linq;

namespace SpiritIsland.SinglePlayer {

	class InvaderPhase : IPhase {

		public string Prompt { get; private set; }
		public IOption[] Options {get; private set;}

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		public event Action<string> NewLogEntry;

		void Log(string msg ) {
			NewLogEntry?.Invoke(msg);
        }

		public InvaderPhase(GameState gameState){
			this.gameState = gameState;
			this.invaderDeck = gameState.InvaderDeck;
		}

		public void Initialize() {
			if(gameState.IsBlighted){
				Log( "Island is blighted" );
				gameState.BlightCard.BlightAction(gameState);
			}

            string[] ravageResults = gameState.Ravage( invaderDeck.Ravage );
			Log("Ravaging:" + (invaderDeck.Ravage?.Text ?? "-") 
				+ "\r\n" + ravageResults.Join("\r\n")
			);
			ResolveCascadingBlight();
		}

		void ResolveCascadingBlight() {
			if(gameState.cascadingBlight.Count > 0) {
				Space space = gameState.cascadingBlight.Pop();
				Prompt = "Select land to cascade blight from " + space.Label;
				Options = space.SpacesExactly( 1 )
					.Where( x => x.Terrain != Terrain.Ocean )
					.ToArray();
			} else
				PostRavage();
		}

		public void Select( IOption option ) {
			Space space = (Space)option;
			gameState.BlightLand(space);
			ResolveCascadingBlight();
		}

		void PostRavage() {
			Log( "Building:" + invaderDeck.Build?.Text ?? "-" );
			var builds = gameState.Build( invaderDeck.Build );
			Log(builds.Join("\r\n"));

			Log( "Exploring:" + invaderDeck.Explore?.Text ?? "-" );
			gameState.Explore( invaderDeck.Explore );

			invaderDeck.Advance();

			Prompt = "nothing to do";
			Options = Array.Empty<IOption>();

			this.Complete?.Invoke();
		}

		public event Action Complete;

	}

}
