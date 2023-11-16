namespace SpiritIsland.NatureIncarnate;

class WatersTasteOfRuin : IHealingCard {
	public string Text => "Waters Taste of Ruin";

	public bool MeetsRequirement( WoundedWatersBleeding spirit )
		=> 3 <= spirit.HealingMarkers[Element.Animal]
		&& 5 <= spirit.HealingMarkers.Total;

	public void Claim( WoundedWatersBleeding spirit ) {
		// Replace Swirl and Spill with Aflict with Bloodthirst
		spirit.InnatePowers[0] = InnatePower.For(typeof(AfflictWithBloodThirst));
		GameState.Current.Log( new Log.LayoutChanged( "Replaced Swirl and Spill with Afflict with Bloodthirst" ) );
		// Replace Seeking a Path Towards Healing
		spirit.StopHealing();
	}
	public bool IsClaimed( WoundedWatersBleeding spirit ) => spirit.InnatePowers[0].Name == AfflictWithBloodThirst.Name;
}
