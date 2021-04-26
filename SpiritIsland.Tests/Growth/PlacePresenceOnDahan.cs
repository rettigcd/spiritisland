namespace SpiritIsland {

	/// <summary>
	/// ThunderSpeakers add-2 (range 1, range 2) 
	/// </summary>
	public class PlacePresenceOnDahan : PlacePresence {
		public PlacePresenceOnDahan(int range):base(range){}

		public override bool IsValid( BoardSpace bs, GameState gs ) => gs.HasDahan(bs);

	}

}
