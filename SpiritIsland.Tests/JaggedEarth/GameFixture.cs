using SpiritIsland.JaggedEarth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Tests.JaggedEarth {
	class GameFixture {

		public GameFixture() {
			spirit = new StonesUnyieldingDefiance();
			boardA = Board.BuildBoardA();
			gameState = new GameState(spirit,boardA);
			user = new VirtualTestUser(gameState.Spirits[0]);
			ravages = new List<InvadersRavaged>();
			gameState.InvadersRavaged.ForEntireGame( (gs,args) => { ravages.Add(args); return Task.CompletedTask; } );
			_ = new SinglePlayer.SinglePlayerGame(gameState); // Start the game 1st, (Initialize will wipe custome invader counts)
		}

			public Spirit spirit;
			public Board boardA;
			public GameState gameState;
			public VirtualTestUser user;
			public List<InvadersRavaged> ravages;
	}

}
