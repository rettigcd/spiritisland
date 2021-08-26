using System;

namespace SpiritIsland {
	public class GameOverException : Exception {
		static public void Win() => throw new GameOverException(WinLoseStatus.Won);
		static public void Lose() => throw new GameOverException( WinLoseStatus.Lost );

		public GameOverException(WinLoseStatus status):base("The game has been " + status ) {
			Status = status;
		}
		public WinLoseStatus Status {get;}
	}

}
