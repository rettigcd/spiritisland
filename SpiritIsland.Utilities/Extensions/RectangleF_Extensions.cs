namespace SpiritIsland;

static public class RectangleFExtensions {

	static public RectangleF Scale( this RectangleF src, float scale ) => new RectangleF( src.X * scale, src.Y * scale, src.Width * scale, src.Height * scale );

	static public SizeF Scale( this SizeF src, float scale ) => new SizeF( src.Width * scale, src.Height * scale );

	static public RectangleF Translate( this RectangleF src, float deltaX, float deltaY ) => new RectangleF( src.X + deltaX, src.Y + deltaY, src.Width, src.Height );

	static public Rectangle ToInts( this RectangleF r ) => new Rectangle( (int)r.X, (int)r.Y, (int)r.Width, (int)r.Height );

	#region RectangleF <==> Bounds
	// RectangleF <==> Bounds
	static public RectangleF ToRectangleF( this Bounds src ) => new RectangleF( src.X, src.Y, src.Width, src.Height );
	static public Bounds ToBounds( this RectangleF src ) => new Bounds( src.X, src.Y, src.Width, src.Height );
	#endregion RectangleF <==> Bounds
}
