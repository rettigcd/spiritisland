namespace SpiritIsland {

	/// <summary>
	/// ThunderSpeakers add-2 (range 1, range 2) 
	/// </summary>
	public class PlacePresenceOnDahan : GrowthAction, IPresenceCriteria{

		public int Range {get;}

		public PlacePresenceOnDahan(int range){ this.Range = range; }

		public override void Apply( PlayerState ps ) {
			ps.PresenceToPlace.Add( this );
		}

		public bool IsValid( BoardSpace bs, GameState gs ) => gs.HasDahan(bs);

	}


}
