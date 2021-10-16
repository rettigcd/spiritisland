using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class GainElement : GrowthActionFactory {

		public Element[] ElementOptions { get; } // public for drawing

		public GainElement(params Element[] elementOptions ) {
			this.ElementOptions = elementOptions;
		}

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var element = await spirit.SelectElement( "Gain element", ElementOptions );
			spirit.Elements[element]++;
		}

		public override string ShortDescription => "GainElement("+ElementOptions.Select(x=>x.ToString()).Join(",")+")";
	}

}
