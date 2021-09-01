using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlacePresence : GrowthActionFactory {

		readonly protected int range;
		readonly protected Target filterEnum;

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

		public override Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return spirit.MakeDecisionsFor(gameState).PlacePresence( range, filterEnum );
		}

	}

}
