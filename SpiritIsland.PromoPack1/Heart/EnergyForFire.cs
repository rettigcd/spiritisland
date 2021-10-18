using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class EnergyForFire : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var presence = ctx.Self.Presence as HeartPresence;
			ctx.Self.Energy += presence.FireShowing();
			return Task.CompletedTask;
		}
	}



}
