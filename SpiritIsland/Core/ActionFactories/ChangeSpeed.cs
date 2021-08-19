using System.Threading.Tasks;

namespace SpiritIsland {

    public class ChangeSpeed : IActionFactory {

		public ChangeSpeed(IActionFactory original, Speed newSpeed){
			Original = original;
			Speed = newSpeed;
		}
		public Speed Speed { get; }

		public string Name => Original.Name;

		public string Text => Original.Text;

		public Task Activate( Spirit spirit, GameState gameState ) {
			return Original.Activate(spirit, gameState);
		}

		public IActionFactory Original { get; }

	}

}
