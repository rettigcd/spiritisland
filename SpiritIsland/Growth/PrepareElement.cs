using System.Threading.Tasks;

namespace SpiritIsland {

	public class PrepareElement : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) => await ctx.Self.PrepareElement();

	}

}
