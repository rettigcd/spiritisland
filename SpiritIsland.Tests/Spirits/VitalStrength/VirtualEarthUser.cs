namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

public static class VirtualEarthUser {

	static public void SelectsGrowthA_Reclaim_PP2(this VirtualUser user) {
		user.Growth_SelectAction( "Place Presence(2)" );
		user.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
	}

	static public void SelectsGrowthB_DrawCard_PP0(this VirtualUser user) {
		user.Growth_DrawsPowerCard();
		user.SelectsMinorDeck();
		user.SelectMinorPowerCard();

		user.Growth_SelectAction( "Place Presence(0)" );
		user.Growth_PlacesEnergyPresence( "A4" );
	}

	static public void SelectsGrowthC_Energy_PP1(this VirtualUser user) {
		user.Growth_SelectAction( "Place Presence(1)" );
		user.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
	}

}

