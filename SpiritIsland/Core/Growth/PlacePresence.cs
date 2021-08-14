using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class PlacePresence : GrowthActionFactory {

		readonly int range;
		readonly Target filterEnum;

		public override string ShortDescription {get;}

		#region constructors

		public PlacePresence( int range ){
			this.range = range;
			filterEnum = Target.Any;
			ShortDescription = $"PlacePresence({range})";
		}

		public PlacePresence(
			int range,
			Target filterEnum,
			string funcDescriptor
		) {
			this.range = range;
			this.filterEnum = filterEnum;
			ShortDescription = $"PlacePresence({range},{funcDescriptor})";
		}

		#endregion

		public override Task Activate( ActionEngine engine ) {
			return engine.PlacePresence( range, filterEnum );
		}

	}

}
