using System.Drawing.Drawing2D;

namespace SpiritIsland;

public abstract class PenSpec {

	/// <param name="format">Examples: Blue, 336677;1.5</param>
	static public implicit operator PenSpec(string format) => new CustomPen( format );
	static public implicit operator PenSpec((string colorName, float width) tuple) => new CustomPen( ColorString.Parse(tuple.colorName),tuple.width );
	static public implicit operator PenSpec((Color color, float width) tuple) => new CustomPen( tuple.color,tuple.width );
	static public implicit operator PenSpec(Color color) => new CustomPen( color );
	static public implicit operator PenSpec(Pen pen) => new BorrowedPen(pen);

	public abstract ResourceMgr<Pen> GetResourceMgr(Rectangle bounds);

	public void Stroke( Graphics graphics, Rectangle rect ){
		using var penMgr = GetResourceMgr( rect );
		graphics.DrawRectangle( penMgr.Resource, rect );
	}

	public void Stroke( Graphics graphics, GraphicsPath path, Rectangle bounds ){
		using var penMgr = GetResourceMgr( bounds );
		graphics.DrawPath( penMgr.Resource, path );
	}

	public void Stroke( Graphics graphics, Point from, Point to, Rectangle bounds ){
		using var penMgr = GetResourceMgr( bounds );
		graphics.DrawLine( penMgr.Resource, from, to );
	}

	class CustomPen : PenSpec{
		public CustomPen(Color color){ _color = color; }
		public CustomPen(Color color, float width){ _color = color; _width = width; }
		public CustomPen(string format){
			string[] parts = format.Split(';');
			_color = ColorString.Parse(parts[0]);
			if(1<parts.Length) _width = float.Parse(parts[1]);
		}
	 	public override ResourceMgr<Pen> GetResourceMgr(Rectangle bounds){
			Pen pen = _width.HasValue 
				? new Pen(_color, Math.Min(bounds.Width,bounds.Height)*_width.Value ) 
				: new Pen(_color);
			return new ResourceMgr<Pen>( pen, true );
		}
		readonly Color _color;
		readonly float? _width;
	}
	class BorrowedPen(Pen pen) : PenSpec {
		public override ResourceMgr<Pen> GetResourceMgr(Rectangle _) => new ResourceMgr<Pen>( pen, false );
	}

}
