namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

public class VirtualEarthUser : VirtualUser {

	public VirtualEarthUser(Spirit spirit ) : base( spirit ) { }

	public void SelectsGrowthA_Reclaim_PP2() {
		Growth_SelectAction( "PlacePresence(2)" );
		Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
	}

	public void SelectsGrowthB_DrawCard_PP0() {
		Growth_DrawsPowerCard();
		SelectsMinorPowerCard();
		SelectsFirstOption( "Select minor Power Card" );

		Growth_SelectAction( "PlacePresence(0)" );
		Growth_PlacesEnergyPresence( "A4" );
	}

	public void SelectsGrowthC_Energy_PP1() {
		Growth_SelectAction( "PlacePresence(1)" );
		Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
	}

}

