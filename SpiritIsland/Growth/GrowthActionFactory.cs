
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Growth;
		public bool IsInactiveAfter( Speed speed ) => speed == Speed.Growth;

		public string Text => this.ShortDescription;

	}

}
