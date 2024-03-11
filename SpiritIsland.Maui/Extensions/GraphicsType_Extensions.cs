//using Android.OS;
namespace SpiritIsland.Maui;

public static class GraphicsType_Extensions {

	static public PointF ToPointF( this XY src ) => new PointF(src.X,src.Y);
	static public Point ToPoint( this XY src ) => new Point( src.X, src.Y );
	static public XY ToXY( this Point src ) => new XY( (float)src.X, (float)src.Y );
	static public XY ToXY( this PointF src ) => new XY( src.X, src.Y );

	static public Bounds ToBounds( this RectF src ) => new Bounds( src.X, src.Y, src.Width, src.Height );

	#region FitXXX(...) methods

	//static public Bounds FitBoth( this Bounds bounds, float widthRatio, Align horizontal = default, Align vertical = default )
	//	=> bounds.Height * widthRatio < bounds.Width
	//		? bounds.FitHeight( new XY( bounds.Height * widthRatio, bounds.Height ), horizontal )
	//		: bounds.FitWidth( new XY( bounds.Width, bounds.Width / widthRatio ), vertical );

	static public Bounds FitBoth( this Bounds bounds, XY size, Align horizontal = default, Align vertical = default )
		=> bounds.Height * size.X < size.Y * bounds.Width
			? bounds.FitHeight( size, horizontal )
			: bounds.FitWidth( size, vertical );

	static public Bounds FitHeight( this Bounds bounds, XY size, Align align = default ) {
		float width = bounds.Height * size.X / size.Y;
		return align switch {
			Align.Near => new Bounds( bounds.X, bounds.Y, width, bounds.Height ),
			Align.Far => new Bounds( bounds.Right - width, bounds.Y, width, bounds.Height ),
			_ => new Bounds( bounds.X + (bounds.Width - width) / 2, bounds.Y, width, bounds.Height ), // center / default
		};
	}

	public static Bounds FitWidth( this Bounds bounds, XY size, Align align = default ) {
		float height = bounds.Width * size.Y / size.X;
		return align switch {
			Align.Near => new Bounds( bounds.X, bounds.Y, bounds.Width, height ),
			Align.Far => new Bounds( bounds.X, bounds.Bottom - height, bounds.Width, height ),
			_ => new Bounds( bounds.X, bounds.Y + (bounds.Height - height) / 2, bounds.Width, height ), // center / default
		};
	}
	#endregion FitXXX(...) methods

}
