using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlayExtraCardThisTurn : GrowthActionFactory {

		readonly int count;
		public PlayExtraCardThisTurn( int count ) {
			this.count = count;
		}

		public override Task ActivateAsync( SelfCtx ctx ) {
			ctx.Self.tempCardPlayBoost += count;
			return Task.CompletedTask;
		}

		public override string Name => $"PlayExtraCardThisTurn({count})";

	}

}
