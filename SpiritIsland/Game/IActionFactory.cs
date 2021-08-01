using SpiritIsland.Core;

namespace SpiritIsland {

	public interface IActionFactory : IOption {
		void Activate(ActionEngine engine);
		Speed Speed { get; }
		string Name { get; }
		IActionFactory Original { get; }
	}

}