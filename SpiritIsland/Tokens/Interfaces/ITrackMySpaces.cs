namespace SpiritIsland;

/// <summary>
/// Any SpaceEntity that Implements this will get called when it is added, removed, initialized, adjusted
/// .Adjust(token,...) to track the spaces & boards the token is on.
/// </summary>
public interface ITrackMySpaces {
	/// <summary>
	/// clear out tracking because Restorer is about to loop through everything and call TrackAdjust();
	/// </summary>
	void Clear();
	void TrackAdjust( Space space, int delta );
}
