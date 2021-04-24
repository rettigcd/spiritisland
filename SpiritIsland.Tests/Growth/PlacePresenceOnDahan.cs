namespace SpiritIsland {

	/// <summary>
	/// ThunderSpeakers add-2 (range 1, range 2) 
	/// </summary>
	public class PlacePresenceOnDahan : GrowthAction{

		public PlacePresenceOnDahan(){}

		public override void Apply( PlayerState ps, GameState gs ) {
			var one = new PresenceCriteria{ Range = 1, IsValid = gs.HasDahan };
			var two = new PresenceCriteria{ Range = 2, IsValid = gs.HasDahan };
			ps.PresenceToPlace.Add( one );
			ps.PresenceToPlace.Add( two );
		}

	}


}
