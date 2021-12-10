using System.Threading.Tasks;

namespace SpiritIsland {
	public interface InnateRepeater {
		Task<bool> ShouldRepeat( Spirit spirit );
	}

}
