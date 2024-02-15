namespace SpiritIsland;

static public class XY_Extensions {

	// to XY
	static public XY ToXY( this Point p ) => new XY( p.X, p.Y );
	static public XY ToXY( this PointF p ) => new XY( p.X, p.Y );

	// to Point
	static public Point ToInts( this PointF src ) => new Point( (int)(src.X + .5f), (int)(src.Y + .5f) );
	static public Point ToInts( this XY src )     => new Point( (int)(src.X + .5f), (int)(src.Y + .5f) );

	// to PointF
	static public PointF ToPointF( this XY p ) => new PointF(p.X,p.Y);

}
