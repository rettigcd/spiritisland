using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction{
		readonly int range;
		public PlacePresence(int range){ this.range = range; }

		public override void Apply( PlayerState ps, GameState _ ) {
			var calc = new PresenceCalculator(ps.Presence,x=>true);
			calc.Execute(range);
			ps.PresenseToPlaceOptions = calc.Results;
		}
	}

	public class PlacePresenceTwice : GrowthAction{

		public override void Apply( PlayerState ps, GameState _ ) {
			var calc = new PresenceCalculator(ps.Presence,x=>true);
			calc.Execute(1,1);
			ps.PresenseToPlaceOptions = calc.Results;
		}
	}


}
