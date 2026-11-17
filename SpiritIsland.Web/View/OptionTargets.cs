namespace SpiritIsland.Web.View;

/// <summary> Works out what each option of a decision points at. Plain engine reads, no web types. </summary>
public static class OptionTargets {

	/// <param name="viewer">The spirit making the decision: what is offered to it (cards, innates, track slots) is its own.</param>
	public static OptionTarget For( IOption option, Spirit viewer ) => option switch {
		Move move => For( move.Source, viewer ) with { Kind = "move", ToSpace = (move.Destination as Space)?.SpaceSpec.Label },
		TargetedAction act => For( act.Action, viewer ) with {
			Kind = "act",
			ToSpace = (act.Target as Space)?.SpaceSpec.Label,
			ToSpirit = (act.Target as Spirit)?.SpiritName,
		},
		Space space => new( "space", Space: space.SpaceSpec.Label ),
		SpaceSpec spec => new( "space", Space: spec.Label ),
		SpaceToken st => new( "spaceToken", Space: st.Space.SpaceSpec.Label, Token: st.Token.Text ),
		TrackPresence tp => OnTrack( "trackPresence", tp.Track, viewer ),
		Track track => OnTrack( "track", track, viewer ),
		DestroyedPresence => new( "destroyedPresence", Spirit: viewer.SpiritName ),
		PowerCard card => new( "card", Spirit: viewer.SpiritName, Card: card.Title ),
		InnatePower innate => new( "innate", Spirit: viewer.SpiritName, Card: innate.Title ),
		IActionFactory action => GrowthTarget( action, viewer ) ?? new( "action" ),
		_ => new( "other" ),
	};

	/// <summary> A growth action is the very object its group holds, so which group and which action it is can be found by identity. </summary>
	static OptionTarget? GrowthTarget( IActionFactory action, Spirit spirit ) {
		GrowthGroup[] groups = spirit.GrowthTrack.Groups;
		for( int g = 0; g < groups.Length; ++g ) {
			int a = Array.IndexOf( groups[g].GrowthActionFactories, action );
			if( 0 <= a ) return new( "growth", Spirit: spirit.SpiritName, Group: g, Action: a );
		}
		return null;
	}

	static OptionTarget OnTrack( string kind, Track track, Spirit spirit ) {
		int energy = IndexOf( spirit.Presence.Energy, track );
		if( 0 <= energy ) return new( kind, Spirit: spirit.SpiritName, Track: "energy", Slot: energy );
		int cards = IndexOf( spirit.Presence.CardPlays, track );
		return 0 <= cards ? new( kind, Spirit: spirit.SpiritName, Track: "cardPlays", Slot: cards ) : new( "other" );
	}

	static int IndexOf( IPresenceTrack track, Track slot ) => track.Slots.ToList().IndexOf( slot );
}
