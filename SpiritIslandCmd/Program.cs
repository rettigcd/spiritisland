using SpiritIsland;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;

namespace SpiritIslandCmd {

	class Program {

		static void Main(string[] _) {

			var gs = new GameState( new RiverSurges() ){ 
				Island = new Island(Board.BuildBoardA())
			};

			var game = new SinglePlayerGame(gs,new ConsoleLogger());

			new CmdLinePlayer(game).Play();
		}

	}

}
