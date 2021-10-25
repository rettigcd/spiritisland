namespace SpiritIsland.Tests.Basegame.Spirits.VitalStrengthNS {

	public class VirtualEarthUser : VirtualUser {

		public VirtualEarthUser(Spirit spirit ) : base( spirit ) { }

		public void SelectsGrowthA_Reclaim_PP2() {
			Growth_SelectsOption( "ReclaimAll / PlacePresence(2)" );
			Growth_ReclaimsAll();
			Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
		}

		public void SelectsGrowthB_DrawCard_PP0() {
			Growth_SelectsOption( "DrawPowerCard / PlacePresence(0)" );
			Growth_DrawsPowerCard();
			Growth_PlacesEnergyPresence( "A4" );
		}

		public void SelectsGrowthC_Energy_PP1() {
			Growth_SelectsOption( "GainEnergy(2) / PlacePresence(1)" );
			Growth_GainsEnergy();
			Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		}

	}

}
