namespace SpiritIsland;

/// <summary>
/// Implemented by SpaceToken and TrackPresence to generically represent some Token at some Location
/// </summary>
public interface TokenOn : IOption {
	IToken Token { get; }
	ILocation Source { get; }
}
