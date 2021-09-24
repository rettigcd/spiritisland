
namespace SpiritIsland.Decision.Presence {

	public class ToRemoveFromTrack : TypedDecision<Track> {
		public ToRemoveFromTrack( Spirit spirit ) : base( "Select Presence to place.", spirit.Presence.GetPlaceableFromTracks(), Present.Always ) { }
	}


}