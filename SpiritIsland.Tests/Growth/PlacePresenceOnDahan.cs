namespace SpiritIsland {

	/// <summary>
	/// ThunderSpeakers add-2 (range 1, range 2) 
	/// </summary>
	public class PlacePresenceOnDahan : GrowthAction{

		public PlacePresenceOnDahan(){}

		public override void Apply( PlayerState ps, GameState gs ) {
			var calc = new PresenceCalculator( ps.Presence, gs.HasDahan );
			calc.Execute(1,2);
			calc.Execute(2,1);
			ps.PresenseToPlaceOptions = calc.Results;
		}

	}


}
