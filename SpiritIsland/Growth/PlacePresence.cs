using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlacePresence : GrowthActionFactory {

		readonly protected int range;
		readonly protected string filterEnum;

		public override string Name {get;}

		#region constructors

		public PlacePresence( int range ){
			this.range = range;
			filterEnum = Target.Any;
			Name = $"PlacePresence({range})";
		}

		public PlacePresence(
			int range,
			string filterEnum
		) {
			this.range = range;
			this.filterEnum = filterEnum;
			Name = $"PlacePresence({range},{filterEnum})";
		}

		#endregion

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) => ctx.PlacePresence( range, filterEnum );

	}

}
