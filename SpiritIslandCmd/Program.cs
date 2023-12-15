using SpiritIsland;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;

namespace SpiritIslandCmd {

	class Program {

		static void Main(string[] _1) {

			var gs = new GameState( new RiverSurges(), Board.BuildBoardA() );

			var game = new SinglePlayerGame(gs);
			_ = game.StartAsync();

			new CmdLinePlayer(game).Play();
		}

	}

}
