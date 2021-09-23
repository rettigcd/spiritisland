namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class VirtualRiverUser : VirtualUser {

		public VirtualRiverUser(Spirit spirit ) : base( spirit ) { }

		public void SelectsGrowthA_Reclaim() {
			SelectsGrowthOption( "ReclaimAll / DrawPowerCard / GainEnergy(1)" );
			ReclaimsAll();
			GainsEnergy();
			DrawsPowerCard();
		}

		public void SelectsGrowthB_2PP(string pp1="energy>A2;A3;A4", string pp2="energy>A1;A2;A3;A4") {
			SelectsGrowthOption( "PlacePresence(1) / PlacePresence(1)" );
			PlacesPresence( pp1 );
			System.Threading.Thread.Sleep(5); // need to let engine place the presence before we read the next spot on the track
			PlacesPresence( pp2 );
		}

		public void SelectsGrowthC_Draw_Energy( string placePresenceOptions = "cardplay>A1;A2;A3;A4;A5" ) {
			SelectsGrowthOption( "DrawPowerCard / PlacePresence(2)" );
			DrawsPowerCard();
			PlacesPresence( placePresenceOptions );
		}

	}

}
