using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class AsyncEvent<T> {

		public async Task InvokeAsync(GameState gameState,T t) {
			foreach(var handler in ForRound)
				await TryHandle( handler, gameState, t );
			foreach(var handler in ForGame)
				await TryHandle( handler, gameState, t );
		}

		static async Task TryHandle( Func<GameState, T, Task> handler, GameState gameState, T t ) {
			try {
				await handler( gameState, t );
			}
			catch(Exception) {
			}
		}
		public void EndOfRound(GameState _) { ForRound.Clear(); }
		public List<Func<GameState,T,Task>> ForRound = new List<Func<GameState, T,Task>>();
		public List<Func<GameState, T, Task>> ForGame = new List<Func<GameState, T, Task>>();
	}

}
