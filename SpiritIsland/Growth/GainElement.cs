using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class GainElement : GrowthActionFactory {

		public Element[] ElementOptions { get; } // public for drawing

		public GainElement(params Element[] elementOptions ) {
			this.ElementOptions = elementOptions;
		}

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var element = await ctx.Self.SelectElement( "Gain element", ElementOptions );
			ctx.Self.Elements[element]++;
		}

		public override string Name => "GainElement("+ElementOptions.Select(x=>x.ToString()).Join(",")+")";
	}

}
