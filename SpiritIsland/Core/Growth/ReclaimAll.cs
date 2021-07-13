
namespace SpiritIsland.Core {
	public class ReclaimAll : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
			return new ResolvedAction();
		}

	}

}
