using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class GrowthOptionGroup {

		public GrowthOption[] Options { get; }
		public GrowthOptionGroup( params GrowthOption[] options ) {
			this.Options = options;
		}

		public GrowthOptionGroup Pick(int selectionCount ) {
			this.SelectionCount = selectionCount;
			return this;
		}
		public int SelectionCount { get; private set; } = 1; // default

	}

}
