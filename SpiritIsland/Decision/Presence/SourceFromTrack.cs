
namespace SpiritIsland.Decision.Presence {

	public class SourceFromTrack : TypedDecision<Track> {
		public SourceFromTrack( Spirit spirit ) 
			: base( "Select Presence to place.", spirit.Presence.GetPlaceableTrackOptions(), "Take Presence from Board" ) 
		{
		}
	}

	public class ReturnToTrackDestination : TypedDecision<Track> {
		public ReturnToTrackDestination( Spirit spirit )
			:base ( "Select Destination to return presence", spirit.Presence.GetReturnableTrackOptions() ) 
		{
		}

}