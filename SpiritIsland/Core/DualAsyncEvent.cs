using System.Threading.Tasks;

namespace SpiritIsland {

	public class DualAsyncEvent<T> {

		public async Task InvokeAsync( T t ) {
			await ForRound.InvokeAsync(t);
			await ForGame.InvokeAsync(t);
		}

		public AsyncEvent<T> ForRound { get; set; } = new SpiritIsland.AsyncEvent<T>();

		public AsyncEvent<T> ForGame { get; set; } = new SpiritIsland.AsyncEvent<T>();

	}


}
