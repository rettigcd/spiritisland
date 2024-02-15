namespace SpiritIsland;

/// <summary>
/// A Rectangle
/// </summary>
/// <remarks>Name chosen so as to not conflict with Windows or Maui</remarks>
public record struct Bounds(float X, float Y, float Width, float Height ) {
	public readonly float Left => X;
	public readonly float Right => X+Width;
	public readonly float Top => Y;
	public readonly float Bottom => Y+Height;
}