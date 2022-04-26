namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class VirtualRiverUser : VirtualUser {

		public VirtualRiverUser(Spirit spirit ) : base( spirit ) { }

		public void SelectsGrowthA_Reclaim() {
			Growth_SelectsOption( "ReclaimAll / DrawPowerCard / GainEnergy(1)" );
//			Growth_ReclaimsAll();
//			Growth_GainsEnergy();
//			Growth_DrawsPowerCard();
		}

		public void SelectsGrowthB_2PP(string pp1="energy>A2;A3;A4", string pp2="energy>A1;A2;A3;A4") {
			Growth_SelectsOption( "PlacePresence(1) / PlacePresence(1)" );
			Growth_PlacesPresence( pp1 );
			Growth_PlacesPresence( pp2 );
		}

		public void SelectsGrowthC_Draw_Energy( string placePresenceOptions = "cardplay>A1;A2;A3;A4;A5" ) {
			Growth_SelectsOption( "DrawPowerCard / PlacePresence(2)" );
			Growth_DrawsPowerCard();
			Growth_PlacesPresence( placePresenceOptions );
		}

	}

}
