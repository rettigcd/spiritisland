using System.Threading.Tasks;

namespace SpiritIsland {
	public class GainEnergyEqualToCardPlays : GrowthActionFactory {
		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.Energy += ctx.Self.Presence.CardPlayCount;
			return Task.CompletedTask;
		}
	}

}
