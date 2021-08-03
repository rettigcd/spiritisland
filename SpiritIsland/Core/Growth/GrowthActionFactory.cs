
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract Task Activate(ActionEngine engine);

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public Speed Speed => Speed.Growth;

		public string Text => this.ShortDescription;

		public IActionFactory Original => this;
	}

}
