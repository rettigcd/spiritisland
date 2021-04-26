namespace SpiritIsland {
	/// <summary>
	/// ThunderSpeakers add-2 (range 1, range 2) 
	/// </summary>
	public class PlacePresenceOnJungleOrWetland : PlacePresence {
		public PlacePresenceOnJungleOrWetland(int range):base(range){}

		public override bool IsValid( BoardSpace bs, GameState gs ) 
			=> bs.Terrain == Terrain.Jungle || bs.Terrain == Terrain.Wetland;

	}


}
