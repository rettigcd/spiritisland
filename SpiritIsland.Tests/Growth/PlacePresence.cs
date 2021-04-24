using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction{
		readonly int range;
		public PlacePresence(int range){ this.range = range; }

		public override void Apply( PlayerState ps, GameState _ ) {
			var place = new PresenceCriteria{ Range = range, IsValid = (b)=>true };
			ps.PresenceToPlace.Add( place );
		}
	}

	public class PlacePresenceTwice : GrowthAction{

		public override void Apply( PlayerState ps, GameState _ ) {
			var single = new PresenceCriteria{ Range = 1, IsValid = (b)=>true };
			ps.PresenceToPlace.Add( single );
			ps.PresenceToPlace.Add( single );
		}
	}


}
