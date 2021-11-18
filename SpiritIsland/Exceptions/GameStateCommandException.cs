using System;

namespace SpiritIsland {

	public class GameStateCommandException : Exception {
		public GameStateCommandException(GameStateCommand cmd) : base() {
			Cmd = cmd;
		}
		public GameStateCommand Cmd { get; }

	}

	public enum GameStateCommand { ReturnToBeginningOfRound }
	
}
