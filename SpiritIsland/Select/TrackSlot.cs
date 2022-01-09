using System.Collections.Generic;

namespace SpiritIsland.Select {

	public class TrackSlot : TypedDecision<Track> {

		static public TrackSlot ToReveal( string prompt, SpiritIsland.Spirit spirit )
			=> new TrackSlot( prompt, spirit.Presence.RevealOptions, "Take Presence from Board" );

		static public TrackSlot ToCover( SpiritIsland.Spirit spirit )
			=> new TrackSlot( "Select Destination to return presence", spirit.Presence.CoverOptions );

		TrackSlot( string prompt, IEnumerable<Track> trackOptions, string cancelOption = null )
			: base( prompt, trackOptions, cancelOption ) {
		}

	}

}