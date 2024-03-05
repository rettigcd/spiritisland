namespace SpiritIsland;

/// <summary>
/// Implemented by SpaceToken and TrackPresence to generically represent some Token at some Location
/// </summary>
public interface TokenLocation : IOption {
	IToken Token { get; }
	ILocation Location { get; }
}
