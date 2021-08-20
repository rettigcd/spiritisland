
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public Speed Speed => Speed.Growth;

		public string Text => this.ShortDescription;

		public IActionFactory Original => this;
	}

}
