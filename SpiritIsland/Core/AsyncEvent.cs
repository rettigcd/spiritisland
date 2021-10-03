using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class AsyncEvent<T> {

		public async Task InvokeAsync(GameState gameState,T t) {
			foreach(var handler in roundActions)
				await TryHandle( handler, gameState, t );
			foreach(var handler in gameActions)
				await TryHandle( handler, gameState, t );
		}

		public void ForThisRound( Func<GameState,T,Task> action ) {
			this.roundActions.Add(action);
		}

		public void ForThisRound( Action<GameState,T> action ) {
			this.roundActions.Add((gs,args)=>{ action(gs,args); return Task.CompletedTask; } );
		}

		public void ForEntireGame( Func<GameState,T,Task> action ) {
			this.gameActions.Add(action);
		}

		public void OnEndOfRound(GameState _) { roundActions.Clear(); }
		readonly List<Func<GameState,T,Task>> roundActions = new List<Func<GameState,T,Task>>();
		readonly List<Func<GameState,T,Task>> gameActions = new List<Func<GameState,T,Task>>();

		static async Task TryHandle( Func<GameState, T, Task> handler, GameState gameState, T t ) {
			try {
				await handler( gameState, t );
			}
			catch(Exception) {
				// !! should do something with this...
			}
		}

	}

}
