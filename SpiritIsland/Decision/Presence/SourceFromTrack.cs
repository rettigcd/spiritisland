﻿
namespace SpiritIsland.Decision.Presence {

	public class SourceFromTrack : TypedDecision<Track> {
		public SourceFromTrack( Spirit spirit ) 
			: base( "Select Presence to place.", spirit.Presence.RevealOptions, "Take Presence from Board" ) 
		{
		}
	}

}