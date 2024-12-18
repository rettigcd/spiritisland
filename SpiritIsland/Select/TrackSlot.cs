namespace SpiritIsland.A;

public class TrackSlot : TypedDecision<Track> {

	/// <summary>
	/// Optional, can choose to Take-From-Board instead.
	/// </summary>
	//static public TrackSlot ToRevealOrTakeFromBoard( string prompt, SpiritIsland.Spirit spirit )
	//	=> new TrackSlot( 
	//		prompt, 
	//		spirit.Presence.RevealOptions(), 
	//		spirit.Presence.CanMove ? "Take Presence from Board" : null // if they cannot take from board, null means they have to choose from Track
	//	);

	/// <summary>
	/// Must choose a Presence to take from Track if possible.
	/// </summary>
	//static public TrackSlot ToReveal( string prompt, SpiritIsland.Spirit spirit )
	//	=> new TrackSlot( prompt, spirit.Presence.RevealOptions(), null	);

	static public TrackSlot ToCover( SpiritIsland.Spirit spirit )
		=> new TrackSlot( "Select Destination to return presence", spirit.Presence.CoverOptions );

	TrackSlot( string prompt, IEnumerable<Track> trackOptions, string? cancelOption = null )
		: base( prompt, trackOptions, cancelOption ) {
	}

}
