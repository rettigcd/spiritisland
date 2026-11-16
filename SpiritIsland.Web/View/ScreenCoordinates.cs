namespace SpiritIsland.Web.View;

/// <summary>
/// The engine's coordinates have y pointing up, as in maths; a screen's y points down. This turns the island over, keeping it
/// inside the same range, which is what the desktop apps' PointMapper does as well. Every position the views send goes through
/// here, so a client can draw the numbers as they are.
/// </summary>
public sealed class ScreenCoordinates( Bounds world ) {

	/// <summary> The world of the game's own spaces: the range every position stays inside. </summary>
	public static ScreenCoordinates For( GameState gs )
		=> new( new BoundsBuilder( gs.SpaceSpecs_Unfiltered.Select( s => s.Layout.Bounds ) ).GetBounds() );

	public Bounds World { get; } = world;

	public PointView Point( XY engine ) => new( engine.X, Y( engine.Y ) );

	public double Y( double engineY ) => World.Y * 2 + World.Height - engineY;
}
