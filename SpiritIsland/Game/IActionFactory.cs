using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {
		Task Activate(ActionEngine engine);
		Speed Speed { get; }
		string Name { get; }
		IActionFactory Original { get; }
	}

}