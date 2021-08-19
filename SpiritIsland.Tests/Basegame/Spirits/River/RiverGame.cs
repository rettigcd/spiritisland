namespace SpiritIsland.Tests.Basegame.Spirits.River {
	public class RiverGame : GameBaseTests {
		protected void Game_PlacePresence1( string sourceTrack, string destinationSpace ) {
			Game_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			Game_SelectOptionContains( "Select Presence to place", sourceTrack );
			Game_SelectOption( "Where would you like", destinationSpace );
		}

		protected void Game_PlacePresence1( Track sourceTrack, string destinationSpace ) {
			Game_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			Game_SelectOption( "Select Presence to place", sourceTrack );
			Game_SelectOption( "Where would you like", destinationSpace );
		}

		protected void Game_Reclaim1( string cardToReclaim ) {
			Game_SelectOption( "Select Growth to resolve", "Reclaim(1)" );
			Game_SelectOption( "Select card to reclaim.", cardToReclaim );
		}

	}

}
