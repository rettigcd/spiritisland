namespace SpiritIsland.Tests.Spirits.River;

static public class VirtualRiverUser_Extensions {

	static public void SelectsGrowthA_Reclaim(this VirtualUser user) {
		user.Growth_DrawsPowerCard();
		user.SelectsMinorDeck();
		user.SelectMinorPowerCard();
	}

	static public void SelectsGrowthB_2PP(this VirtualUser user, string pp1 = "energy>A2;A3;A4", string pp2 = "energy>A1;A2;A3;A4") {
		user.Growth_PlacesPresence(pp1);
		user.Growth_PlacesPresence(pp2);
	}

	static public void SelectsGrowthC_Draw_Energy(this VirtualUser user, string placePresenceOptions = "cardplay>A1;A2;A3;A4;A5") {
		user.Growth_SelectAction("Place Presence(2)");
		user.Growth_PlacesPresence(placePresenceOptions);
		user.Growth_SelectAction("Gain Power Card");
		user.SelectsMinorDeck();
		user.SelectMinorPowerCard();
	}

}