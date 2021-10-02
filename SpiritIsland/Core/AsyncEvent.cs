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

		public void Add( Func<GameState,T,Task> action ) {
			this.ForRound.Add(action);
		}

		public void Add( Action<GameState,T> action ) {
			this.ForRound.Add((gs,args)=>{ action(gs,args); return Task.CompletedTask; } );
		}

		public void Always( Func<GameState,T,Task> action ) {
			this.ForGame.Add(action);
		}

		public void OnEndOfRound(GameState _) { ForRound.Clear(); }
		readonly List<Func<GameState,T,Task>> ForRound = new List<Func<GameState,T,Task>>();
		readonly List<Func<GameState,T,Task>> ForGame = new List<Func<GameState,T,Task>>();

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
