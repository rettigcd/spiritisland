
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ReclaimAll : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var spirit = ctx.Self;
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
			return Task.CompletedTask;
		}

	}

}
