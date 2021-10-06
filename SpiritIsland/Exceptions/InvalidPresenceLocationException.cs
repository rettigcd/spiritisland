using System;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland {
	public class InvalidPresenceLocationException : Exception {

		public InvalidPresenceLocationException(string invalidSpace,string[] allowed)
			:base($"Invalid:{invalidSpace} allowed:"+allowed.Join(","))
		{}
		
	}

	public class GameStateCommandException : Exception {
		public GameStateCommandException(GameStateCommand cmd) : base() {
			Cmd = cmd;
		}
		public GameStateCommand Cmd { get; }

	}

	public enum GameStateCommand { ReturnToBeginningOfRound }
	
}
