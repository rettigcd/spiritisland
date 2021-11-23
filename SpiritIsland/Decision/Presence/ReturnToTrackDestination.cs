
namespace SpiritIsland.Decision.Presence {
	public class ReturnToTrackDestination : TypedDecision<Track> {
		public ReturnToTrackDestination( Spirit spirit )
			:base ( "Select Destination to return presence", spirit.Presence.GetReturnableTrackOptions() ) 
		{
		}
	}

}