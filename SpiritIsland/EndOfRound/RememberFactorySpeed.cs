using System.Threading.Tasks;

namespace SpiritIsland {
	/// <summary> This provides a javascript-like closure to capture the factory that needs reset to fast;</summary>
	public class RememberFactorySpeed {
		readonly IActionFactory factory;
		readonly Speed originalSpeed;
		public RememberFactorySpeed( IActionFactory factory ) {
			this.factory = factory;
			this.originalSpeed = factory.Speed;
		}
		public Task Reset( GameState _ ) {
			factory.Speed = originalSpeed;
			return Task.CompletedTask;
		}
	}


}