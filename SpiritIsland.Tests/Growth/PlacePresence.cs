using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction, IPresenceCriteria{
		public int Range {get;}
		public PlacePresence(int range){ this.Range = range; }

		public bool IsValid(BoardSpace bs, GameState gs) => true;

		public override void Apply( PlayerState ps ) {
			ps.PresenceToPlace.Add( this );
		}
	}

}
