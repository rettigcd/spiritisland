using SpiritIsland;
using SpiritIsland.Invaders;
using System;

namespace SpiritIslandCmd {

	public class GamePlayer {
		readonly IPhase selectGrowth;
		readonly IPhase resolveGrowth;
		readonly IPhase selectPowerCards;
		readonly IPhase fastActions;
		readonly IPhase invaders;
		readonly IPhase slowActions;
		readonly Spirit spirit;
		readonly GameState gameState;

		IPhase phase;

		public GamePlayer(){
			spirit = new RiverSurges();
			Board board = Board.BuildBoardA();
			spirit.InitializePresence(board);
			gameState = new GameState(spirit){ Island = new Island(board)};
			gameState.InitBoards();
			var invaderDeck = new InvaderDeck();
			gameState.Explore(invaderDeck.Explore);
			invaderDeck.Advance();

			selectGrowth = new SelectGrowth(spirit,gameState);
			resolveGrowth = new ResolveGrowth(spirit,gameState);
			selectPowerCards = new SelectPowerCards(spirit);
			fastActions = new ResolveFastSlow(spirit,gameState,Speed.Fast);
			invaders = new FakePhase("Invaders");
			slowActions = new ResolveFastSlow(spirit,gameState,Speed.Slow);

			selectGrowth.Complete += () => TransitionTo(resolveGrowth);
			resolveGrowth.Complete += () => TransitionTo(selectPowerCards);
			selectPowerCards.Complete += () => TransitionTo(fastActions);
			fastActions.Complete += () => TransitionTo(invaders);
			invaders.Complete += () => TransitionTo(slowActions);
			slowActions.Complete += () => TransitionTo(selectGrowth);

		}

		void TransitionTo(IPhase phase){
			this.phase = phase; // ! this must go first! because .Initialize might trigger the next phase
			phase.Initialize();
		}

		public void Play(){

			TransitionTo( selectGrowth );

			while(phase != null){
				Console.WriteLine(phase.Prompt);
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower();
				int index = int.TryParse(cmd,out int t) ? t : 0;
				bool _ = phase.Handle(cmd,index-1)
					|| Generic(cmd);
			}
		}

		bool Generic(string cmd){
			switch(cmd){
				case "spirit":
					Console.WriteLine($"Spirit: {spirit.Text} ET:{spirit.EnergyPerTurn} CT:{spirit.NumberOfCardsPlayablePerTurn}" );
					return true;
				case "board":
					Console.WriteLine($"Board: {gameState.Island.Boards[0][0].Label}" );
					return true;
			}
			return false;
		}

	}

	public class FakePhase : IPhase {

		public FakePhase(string phaseName){
			Prompt = phaseName + " Press 'n' to go to next phase";
		}

		public string Prompt {get;}

		public event Action Complete;

		public bool Handle( string cmd,int _ ) {
			if(cmd != "n") return false;
			this.Complete?.Invoke();
			return true;
		}

		public void Initialize() {
		}
	}

}
