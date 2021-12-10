
namespace SpiritIsland.Decision.Presence {

	public class SourceFromTrack : TypedDecision<Track> {

		public SourceFromTrack( string prompt, Spirit spirit ) 
			: base( prompt, spirit.Presence.RevealOptions, "Take Presence from Board" ) 
		{
		}

	}

}