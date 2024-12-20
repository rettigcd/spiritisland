namespace SpiritIsland;

/// <summary>
/// Implemented by SpaceToken and TrackPresence to generically represent some Token at some Location
/// </summary>
public interface ITokenLocation : IOption {
	IToken Token { get; }
	ILocation Location { get; }

	/// <summary> The current # of given-tokens on this space. </summary>
	int Count { get; }

	/// <summary> Sprits Presences return this value.  Other tokens return false. </summary>
	/// <remarks> Used by the view model to display Sacred Sites.</remarks>
	bool IsSacredSite { get; }
}
