using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class AsyncEvent<T> {

		public Guid Add( Func<GameState, T, Task> action ) {
			Guid key = Guid.NewGuid();
			this.handlers.Add( key, action );
			return key;
		}

		public Guid Add( Action<GameState, T> action ) {
			return this.Add( ( gs, args ) => { action( gs, args ); return Task.CompletedTask; } );
		}

		public void Remove( Guid guid ) => handlers.Remove( guid );

		public async Task InvokeAsync( GameState gameState, T t ) {
			foreach(var handler in handlers.Values)
				await TryHandle( handler, gameState, t );
		}

		public void Clear(GameState _) => handlers.Clear();

		static async Task TryHandle( Func<GameState, T, Task> handler, GameState gameState, T t ) {
			try {
				await handler( gameState, t );
			}
			catch(Exception) {
				// !! should do something with this...
			}
		}

		readonly Dictionary<Guid, Func<GameState, T, Task>> handlers = new Dictionary<Guid, Func<GameState, T, Task>>();
	}


}
