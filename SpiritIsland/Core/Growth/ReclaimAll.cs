
using System.Threading.Tasks;

namespace SpiritIsland {
	public class ReclaimAll : GrowthActionFactory {

		public override Task Activate( Spirit spirit, GameState _ ) {
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
			return Task.CompletedTask;
		}

	}

}
