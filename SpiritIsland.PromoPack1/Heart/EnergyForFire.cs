using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class EnergyForFire : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.Energy += ctx.Self.Presence.AddElements()[Element.Fire];
			return Task.CompletedTask;
		}

	}

}
