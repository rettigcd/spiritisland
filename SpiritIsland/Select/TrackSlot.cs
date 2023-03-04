namespace SpiritIsland.Select;

public class TrackSlot : TypedDecision<Track> {

	static public TrackSlot ToReveal( string prompt, Spirit spirit )
		=> new TrackSlot( prompt, spirit.Presence.RevealOptions(), spirit.Presence.CanMove ? "Take Presence from Board" : null );

	static public TrackSlot ToCover( Spirit spirit )
		=> new TrackSlot( "Select Destination to return presence", spirit.Presence.CoverOptions );

	TrackSlot( string prompt, IEnumerable<Track> trackOptions, string cancelOption = null )
		: base( prompt, trackOptions, cancelOption ) {
	}

}