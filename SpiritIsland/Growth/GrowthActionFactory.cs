
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public Speed Speed { 
			get{ return Speed.Growth; }
			set { throw new System.InvalidOperationException("can't change growth speed"); }
		}

		public string Text => this.ShortDescription;

		public IActionFactory Original => this;
	}

}
