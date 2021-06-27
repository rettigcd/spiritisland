using System.Collections.Generic;

namespace SpiritIsland.Core {

	public interface IResolver {

		void Apply( List<IAction> growthActions );

	}

}
