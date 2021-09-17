
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public Speed Speed => DefaultSpeed;
		public Speed DefaultSpeed => Speed.Growth;
		public SpeedOverride OverrideSpeed { 
			get => null;
			set => throw new System.InvalidOperationException();
		}

		public string Text => this.ShortDescription;

//		public IActionFactory Original => this;

	}

}
