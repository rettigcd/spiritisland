namespace SpiritIsland.NatureIncarnate;

class WatersRenew : IHealingCard {
	public string Text => "Waters Renew";

	public bool MeetsRequirement( WoundedWatersBleeding spirit )
		=> 3 <= spirit.HealingMarkers[Element.Water]
		&& 5 <= spirit.HealingMarkers.Total;

	public void Claim( WoundedWatersBleeding spirit ) {
		// Replace Sanguinary Taint with Call to a fastness of renewal
		spirit.InnatePowers[1] = InnatePower.For(typeof(CallToAFastnessOfRenewal));
		GameState.Current.Log( new Log.LayoutChanged("Replaced Sanguinary Tain with Call to a Fastness of Renwal"));
		// Replace Seeking a Path Towards Healing
		spirit.StopHealing();
	}

	public bool IsClaimed( WoundedWatersBleeding spirit ) => spirit.InnatePowers[1].Name == CallToAFastnessOfRenewal.Name;

}