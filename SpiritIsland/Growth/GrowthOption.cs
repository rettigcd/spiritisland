
namespace SpiritIsland {

	public class GrowthOption{

		public GrowthOption(params GrowthAction[] actions){ this.GrowthActions = actions; }

		public GrowthAction[] GrowthActions { get; }

		internal void Apply( PlayerState playerState ) {
			foreach(var action in GrowthActions)
				action.Apply(playerState);
		}

	}

}
