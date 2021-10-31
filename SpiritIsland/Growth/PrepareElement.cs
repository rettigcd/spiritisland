using System.Threading.Tasks;

namespace SpiritIsland {

	public class PrepareElement : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var el = await ctx.Self.SelectElement("Prepare Element", ElementList.AllElements);
			ctx.Self.PreparedElements[el]++;
		}

	}

}
