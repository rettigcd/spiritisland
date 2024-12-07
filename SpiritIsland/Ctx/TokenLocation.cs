namespace SpiritIsland;

/// <summary>
/// Implemented by SpaceToken and TrackPresence to generically represent some Token at some Location
/// </summary>
public interface TokenLocation : IOption {
	IToken Token { get; }
	ILocation Location { get; }
	int Count { get; }
	/// <summary> Sprits Presences return this value.  Other tokens return false. </summary>
	bool IsSacredSite { get; }
}
