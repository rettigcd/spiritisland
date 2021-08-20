using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {
		Task ActivateAsync(Spirit spirit, GameState gameState);
		Speed Speed { get; }
		string Name { get; }
		IActionFactory Original { get; }
	}

}