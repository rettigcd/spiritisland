using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ExaltationOfMoltenStone {

		[SpiritCard("Exaltation of Molten Stone",1, Element.Moon,Element.Fire,Element.Earth), Fast, AnotherSpirit]
		public static Task ActAsync(TargetSpiritCtx _ ) { 
			// Split 1 Energy per fire you have between yourself and target Spirit, as evenly as possible.
			// Target Spirit gains +1 range with their Powers that originate from a Mountain
			return Task.CompletedTask;
		}

	}

}
