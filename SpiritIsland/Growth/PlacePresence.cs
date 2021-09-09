using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlacePresence : GrowthActionFactory {

		readonly protected int range;
		readonly protected string filterEnum;

		public override string ShortDescription {get;}

		#region constructors

		public PlacePresence( int range ){
			this.range = range;
			filterEnum = Target.Any;
			ShortDescription = $"PlacePresence({range})";
		}

		public PlacePresence(
			int range,
			string filterEnum
		) {
			this.range = range;
			this.filterEnum = filterEnum;
			ShortDescription = $"PlacePresence({range},{filterEnum})";
		}

		#endregion

		public override Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return spirit.MakeDecisionsFor(gameState).PlacePresence( range, filterEnum );
		}

	}

}
