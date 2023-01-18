using System.Drawing;

namespace SpiritIsland.WinForms;

static public class RectangleFExtensions {
	static public Rectangle ToInts( this RectangleF r ) => new Rectangle( (int)r.X, (int)r.Y, (int)r.Width, (int)r.Height );

	static public RectangleF Scale( this RectangleF src, float scale ) => new RectangleF( src.X * scale, src.Y * scale, src.Width * scale, src.Height * scale );

	static public SizeF Scale( this SizeF src, float scale ) => new SizeF( src.Width * scale, src.Height * scale );

	static public RectangleF Translate( this RectangleF src, float deltaX, float deltaY ) => new RectangleF( src.X + deltaX, src.Y + deltaY, src.Width, src.Height );

}
