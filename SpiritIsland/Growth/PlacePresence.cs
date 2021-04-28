using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction, IPresenceCriteria{
		public int Range {get;}
		public PlacePresence(int range){ this.Range = range; }

		public virtual bool IsValid(Space bs, GameState gs) => true;

		public override void Apply( Spirit ps ) {
			ps.PresenceToPlace.Add( this );
		}
	}

}
