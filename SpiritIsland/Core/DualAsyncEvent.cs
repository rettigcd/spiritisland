using System.Threading.Tasks;

namespace SpiritIsland {

	public class DualAsyncEvent<T> {

		public async Task InvokeAsync(GameState gameState,T t) {
			await ForRound.InvokeAsync(gameState, t);
			await ForGame.InvokeAsync(gameState, t);
		}

		public AsyncEvent<T> ForRound { get; set; } = new SpiritIsland.AsyncEvent<T>();

		public AsyncEvent<T> ForGame { get; set; } = new SpiritIsland.AsyncEvent<T>();

	}


}
