namespace SpiritIsland;

/// <summary>
/// A Presence Token on a particular Track(Slot)
/// </summary>
/// <remarks>
/// Matches SpaceToken to make selecting presence generic/uniform.
/// </remarks>
public class TrackPresence : TokenOn {

	#region constructor
	public TrackPresence(Track track,SpiritPresenceToken token ) {
		Track = track;
		Token = token;
	}
	#endregion constructor

	public IToken Token {get;}
	public Track Track {get;}

	#region GetHashCode/Equal
	public override int GetHashCode() => Track.GetHashCode();
	public override bool Equals( object obj ) => obj is TrackPresence tp && Track.Equals( tp.Track );
	#endregion

	#region TokenOn
	ILocation TokenOn.Source => Track;
	string IOption.Text => Track.Text;
	#endregion TokenOn
}