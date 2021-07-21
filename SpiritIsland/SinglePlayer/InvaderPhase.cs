using System;
using System.Linq;

namespace SpiritIsland.SinglePlayer {

	class InvaderPhase : IPhase {

		public string Prompt { get; private set; }
		public IOption[] Options {get; private set;}

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;
		readonly ILogger logger;

		public InvaderPhase(GameState gameState,ILogger logger){
			this.gameState = gameState;
			this.invaderDeck = gameState.InvaderDeck;
			this.logger = logger;
		}

		public void Initialize() {
			if(gameState.IsBlighted){
				logger.Log( "Island is blighted" );
				gameState.BlightCard.BlightAction(gameState);
			}

			var ravageResults = gameState.Ravage( invaderDeck.Ravage );
			logger.Log("Ravaging:" + (invaderDeck.Ravage?.Text ?? "-") 
				+ "\r\n" + ravageResults.Join("ravageResults")
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
			logger.Log( "Building:" + invaderDeck.Build?.Text ?? "-" );
			gameState.Build( invaderDeck.Build );

			logger.Log( "Exploring:" + invaderDeck.Explore?.Text ?? "-" );
			gameState.Explore( invaderDeck.Explore );

			invaderDeck.Advance();

			Prompt = "nothing to do";
			Options = Array.Empty<IOption>();

			this.Complete?.Invoke();
		}

		public event Action Complete;

	}

}
