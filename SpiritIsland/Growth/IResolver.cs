
using SpiritIsland.PowerCards;
using System.Collections.Generic;

namespace SpiritIsland {

	public interface IResolver {

		void Apply( List<IAction> growthActions );

	}

}
