
namespace SpiritIsland.Decision.Presence {

	public class Source : TypedDecision<Track> {
		public Source( Spirit spirit ) 
			: base( "Select Presence to place.", spirit.Presence.GetPlaceableTrackOptions(), "Take Presence from Board" ) 
		{
		}
	}

	public class TakeFromBoard : TypedDecision<Space> {
		public TakeFromBoard( Spirit spirit ) 
			: base( "Select Presence to place.", spirit.Presence.Spaces, Present.Always ) 
		{
		}
	}


}