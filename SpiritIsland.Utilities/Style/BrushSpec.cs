using System.Drawing.Drawing2D;

namespace SpiritIsland;


public abstract class BrushSpec {

	static public implicit operator BrushSpec(string hexColor) => new ColoredBrush( ColorString.Parse(hexColor));
	static public implicit operator BrushSpec(Color color) => new ColoredBrush(color);
	static public implicit operator BrushSpec(Brush brush) => new BorrowedBrush(brush);

	public abstract ResourceMgr<Brush> GetResourceMgr();

	public void Fill( Graphics graphics, Rectangle rect ){
		using var brush = GetResourceMgr();
		graphics.FillRectangle( brush.Resource, rect );
	}

	public void Fill( Graphics graphics, GraphicsPath path ){
		using var brush = GetResourceMgr();
		graphics.FillPath( brush.Resource, path );
	}

	class ColoredBrush(Color color) : BrushSpec{
		public override ResourceMgr<Brush> GetResourceMgr() => new ResourceMgr<Brush>( new SolidBrush(color), true);
	}
	class BorrowedBrush(Brush brush) : BrushSpec {
		public override ResourceMgr<Brush> GetResourceMgr() => new ResourceMgr<Brush>( brush, false );
	}

}
