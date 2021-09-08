
using System.Threading.Tasks;

namespace SpiritIsland {
	public class ReclaimAll : GrowthActionFactory {

		public override Task ActivateAsync( Spirit spirit, GameState _ ) {
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
			return Task.CompletedTask;
		}

	}

}
