using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <remarks>Lure of the Deep Wilderness - Air/Plant/Moon</remarks>
	public class Gain1Element : GrowthActionFactory {

		public Element[] ElementOptions { get; } // public for drawing

		public Gain1Element(params Element[] elementOptions ) {
			this.ElementOptions = elementOptions;
		}

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var element = await ctx.Self.SelectElement( "Gain element", ElementOptions );
			ctx.Self.Elements[element]++;
		}

		public override string Name => "GainElement("+ElementOptions.Select(x=>x.ToString()).Join(",")+")";
	}


}
