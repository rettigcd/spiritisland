
namespace SpiritIsland {
	public class GrowthOption{
		public GrowthAction[] GrowthActions { get; set; }

		internal void Apply( PlayerState playerState, GameState gs ) {
			foreach(var action in GrowthActions)
				action.Apply(playerState, gs);
		}
	}



}
