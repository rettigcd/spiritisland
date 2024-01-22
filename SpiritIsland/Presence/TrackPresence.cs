namespace SpiritIsland;

/// <summary>
/// A Presence Token on a particular Track(Slot)
/// </summary>
/// <remarks>
/// Matches SpaceToken to make selecting presence generic/uniform.
/// </remarks>
public class TrackPresence( Track track, SpiritPresenceToken token ) : TokenOn {

	public IToken Token { get; } = token;
	public Track Track { get; } = track;

	#region GetHashCode/Equal
	public override int GetHashCode() => Track.GetHashCode();
	public override bool Equals( object obj ) => obj is TrackPresence tp && Track.Equals( tp.Track );
	#endregion

	#region TokenOn
	ILocation TokenOn.Source => Track;
	string IOption.Text => Track.Text;
	#endregion TokenOn
}