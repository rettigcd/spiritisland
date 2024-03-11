namespace SpiritIsland;

/// <summary>
/// A Presence Token on a particular Track(Slot)
/// </summary>
/// <remarks>
/// Matches SpaceToken to make selecting presence generic/uniform.
/// </remarks>
public class TrackPresence( Track track, SpiritPresenceToken token ) : TokenLocation {

	public Track Track { get; } = track;
	public IToken Token { get; } = token;

	#region GetHashCode/Equal
	public override int GetHashCode() => Track.GetHashCode();
	public override bool Equals( object obj ) => obj is TrackPresence tp && Track.Equals( tp.Track );
	#endregion

	#region TokenLocation
	ILocation TokenLocation.Location => Track;
	string IOption.Text => Track.Code;

	public int Count => (_pres.Energy.Revealed.Contains(Track) || _pres.CardPlays.Revealed.Contains(Track)) ? 0 : 1;
	bool TokenLocation.IsSacredSite => false;

	#endregion TokenLocation

	readonly SpiritPresence _pres = token.Self.Presence;
}