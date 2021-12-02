using System;

namespace SpiritIsland {

	public class GameStateCommandException : Exception {
		public GameStateCommandException(IGameStateCommand cmd) : base() {
			Cmd = cmd;
		}
		public IGameStateCommand Cmd { get; }

	}

	public interface IGameStateCommand {}
	
	public class Rewind : IGameStateCommand {
		public int TargetRound { get; }
		public Rewind(int toRound ) { TargetRound = toRound; }
	}

}
