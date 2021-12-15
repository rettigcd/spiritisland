using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class GainElements : GrowthActionFactory {

		public Element[] ElementsToGain { get; } // public for drawing

		public GainElements(params Element[] elementsToGain ) {
			this.ElementsToGain = elementsToGain;
		}

		public override Task ActivateAsync( SelfCtx ctx ) {
			ctx.Self.Elements.AddRange(ElementsToGain);
			return Task.CompletedTask;
		}

		public override string Name => "GainElements("+ElementsToGain.Select(x=>x.ToString()).Join(",")+")";
	}


}
