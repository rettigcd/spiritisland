
namespace SpiritIsland {
	public class GrowthOption{

		public GrowthOption(params GrowthAction[] actions){ this.GrowthActions = actions; }

		public GrowthAction[] GrowthActions { get; set; }

		internal void Apply( PlayerState playerState, GameState gs ) {
			foreach(var action in GrowthActions)
				action.Apply(playerState, gs);
		}
	}



}
