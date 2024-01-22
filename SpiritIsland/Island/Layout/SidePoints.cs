using System.Drawing;

namespace SpiritIsland;

public class SidePoints( PointF from, PointF to ) {
	public PointF From = from;
	public PointF To = to;
	public double RotationDegrees => Math.Atan2( To.Y - From.Y, To.X - From.X ) * 180 / Math.PI;


}
