using SpiritIsland;
using System;

namespace SpiritIslandCmd {

	class Program {
		static void Main(string[] args) {
			Console.WriteLine("Hello World!");

			var phase = Phase.Growth;

			var spirit = new RiverSurges();
			var board = Board.BuildBoardA();
			var gameState = new GameState(spirit){ Island = new Island(board)};

			while(phase != Phase.Done){
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower();

				switch(cmd){
					case "spirit":
						Console.WriteLine($"Spirit: {spirit.Text} ET:{spirit.EnergyPerTurn} CT:{spirit.NumberOfCardsPlayablePerTurn}" );
						break;
					case "board":
						Console.WriteLine($"Board: {board[0].Label}" );
						break;
				}

			}


		}

	}

	public enum Phase { Growth, Fast, Invader, Slow, TimePasses, Done }

}
