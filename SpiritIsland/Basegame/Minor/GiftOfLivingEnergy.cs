using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class GiftOfLivingEnergy {
		[MinorCard( "Gift of Living Energy", 0, Speed.Fast, Element.Air, Element.Fire, Element.Plant )]
		[TargetSpirit]
		public static Task ActAsync( ActionEngine eng, Spirit target ) {
			++target.Energy;
			if(target != eng.Self)
				++target.Energy;
			if(eng.Self.SacredSites.Count() >= 2)
				++target.Energy;
			return Task.CompletedTask;
		}
	}


}
