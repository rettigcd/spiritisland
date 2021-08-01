
namespace SpiritIsland.Core {
	public class ReclaimAll : GrowthActionFactory {

		public override void Activate( ActionEngine engine ) {
			var spirit = engine.Self;
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
		}

	}

}
