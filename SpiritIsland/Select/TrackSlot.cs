
namespace SpiritIsland.Select {

	public class TrackSlot : TypedDecision<Track> {

		static public TrackSlot ToReveal( string prompt, SpiritIsland.Spirit spirit )
			=> new TrackSlot( prompt, spirit, "Take Presence from Board" );

		static public TrackSlot ToCover( SpiritIsland.Spirit spirit )
			=> new TrackSlot( "Select Destination to return presence", spirit );

		public TrackSlot( string prompt, SpiritIsland.Spirit spirit, string cancelPrompt = null ) 
			: base( prompt, spirit.Presence.RevealOptions, cancelPrompt ) 
		{
		}

	}

}