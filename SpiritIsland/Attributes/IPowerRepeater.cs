using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IPowerRepeater {
		Task<bool> ShouldRepeat( Spirit spirit );
	}

}
