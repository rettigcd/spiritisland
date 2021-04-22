namespace SpiritIsland {
	public class ReclaimAll : GrowthAction {
		public override void Apply(PlayerState ps, GameState _){
			ps.AvailableCards.AddRange( ps.PlayedCards );
			ps.PlayedCards.Clear();
		}
	}


}
