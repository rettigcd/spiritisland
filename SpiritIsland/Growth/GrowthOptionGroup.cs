using System.Linq;

namespace SpiritIsland {

	public class GrowthOptionGroup {

		public GrowthOption[] Options { get; private set; }

		public GrowthOptionGroup( params GrowthOption[] options ) {
			this.Options = options;
		}

		public void Add( GrowthOption option ) { // hook for Starlight
			var options = Options.ToList();
			options.Add(option);
			Options = options.ToArray();
		}

		public GrowthOptionGroup Pick(int selectionCount ) {
			this.SelectionCount = selectionCount;
			return this;
		}
		public int SelectionCount { get; private set; } = 1; // default

	}

}
