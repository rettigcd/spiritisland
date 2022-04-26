using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Tests {

	class GameFixture {

		public Spirit spirit;
		public Board board;
		public GameState gameState;
		public VirtualUser user;
		public List<InvadersRavaged> ravages;
		public List<string> Log;
		public string LogAsString => string.Join("\r\n",Log);


		public GameFixture WithSpirit(Spirit spirit ) {
			this.spirit = spirit;
			return this;
		}

		public GameFixture Start() {
			if(spirit == null)
				spirit = new TestSpirit(PowerCard.For<WashAway>());
			if(board == null)
				board = Board.BuildBoardA();

			gameState = new GameState(spirit,board);

			// Logging
			ravages = new List<InvadersRavaged>();
			gameState.InvadersRavaged.ForGame.Add( (args) => { ravages.Add(args); return Task.CompletedTask; } );
			Log = new List<string>();
			gameState.NewLogEntry += (e) => Log.Add(e.Msg());


			user = new VirtualUser(spirit);
			_ = new SinglePlayer.SinglePlayerGame(gameState); // Start the game 1st, (Initialize will wipe custome invader counts)
			return this;
		}

		public TargetSpaceCtx TargetSpace( string spaceLabel ) => spirit.Bind( this.gameState )
			.Target( gameState.Island.AllSpaces.Single( x => x.Label == spaceLabel ) );

	}

}
