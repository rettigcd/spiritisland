namespace SpiritIsland {
	public class ReclaimAll : GrowthAction {
		public override void Apply( PlayerState ps ){
			ps.AvailableCards.AddRange( ps.PlayedCards );
			ps.PlayedCards.Clear();
		}
	}


}
