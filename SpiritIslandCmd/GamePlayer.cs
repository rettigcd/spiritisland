using SpiritIsland;
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

			selectGrowth = new SelectGrowth(spirit,gameState);
			resolveGrowth = new ResolveGrowth(spirit,gameState);
			selectPowerCards = new SelectPowerCards(spirit);
			fastActions = new FakePhase("Fast");
			invaders = new FakePhase("Invaders");
			slowActions = new FakePhase("Slow");

			selectGrowth.Complete += () => TransitionTo(resolveGrowth);
			resolveGrowth.Complete += () => TransitionTo(selectPowerCards);
			selectPowerCards.Complete += () => TransitionTo(fastActions);
			fastActions.Complete += () => TransitionTo(invaders);
			invaders.Complete += () => TransitionTo(slowActions);
			slowActions.Complete += () => TransitionTo(selectGrowth);

		}

		void TransitionTo(IPhase phase){
			phase.Initialize();
			this.phase = phase;
		}

		public void Play(){

			TransitionTo( selectGrowth );

			while(phase != null){
				Console.WriteLine(phase.Prompt);
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower();
				bool _ = phase.Handle(cmd)
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

		public bool Handle( string cmd ) {
			if(cmd != "n") return false;
			this.Complete?.Invoke();
			return true;
		}

		public void Initialize() {
		}
	}

}
