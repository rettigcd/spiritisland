using System.Drawing;

namespace SpiritIsland;

public class SidePoints {
	public SidePoints( PointF from, PointF to ) {
		From = from;
		To = to;
	}
	public PointF From;
	public PointF To;
	public double RotationDegrees => Math.Atan2( To.Y - From.Y, To.X - From.X ) * 180 / Math.PI;


}
