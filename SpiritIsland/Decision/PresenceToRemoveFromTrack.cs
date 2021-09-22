
namespace SpiritIsland.Decision {

	public class PresenceToRemoveFromTrack : Decision.TypedDecision<Track> {
		public PresenceToRemoveFromTrack( Spirit spirit ) : base( "Select Presence to place.", spirit.Presence.GetPlaceableFromTracks(), Present.Always ) { }
	}

}