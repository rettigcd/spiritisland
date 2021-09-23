namespace SpiritIsland.Tests.Basegame.Spirits.VitalStrengthNS {

	public class VirtualEarthUser : VirtualUser {

		public VirtualEarthUser(Spirit spirit ) : base( spirit ) { }

		public void SelectsGrowthA_Reclaim_PP2() {
			SelectsGrowthOption( "ReclaimAll / PlacePresence(2)" );
			ReclaimsAll();
			PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
		}

		public void SelectsGrowthB_DrawCard_PP0() {
			SelectsGrowthOption( "DrawPowerCard / PlacePresence(0)" );
			DrawsPowerCard();
			PlacesEnergyPresence( "A4" );
		}

		public void SelectsGrowthC_Energy_PP1() {
			SelectsGrowthOption( "GainEnergy(2) / PlacePresence(1)" );
			GainsEnergy();
			PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		}

	}

}
