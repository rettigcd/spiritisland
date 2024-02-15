namespace SpiritIsland;

public record SidePoints( XY From, XY To ) {
	public double RotationDegrees => Math.Atan2( To.Y - From.Y, To.X - From.X ) * 180 / Math.PI;
}
