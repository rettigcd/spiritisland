using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// "Pool" - Holds "floating" rects
/// </summary>
public class PoolRect : IPaintableRect {
	readonly List<IPaintableRect> _paintables = [];
	readonly List<RectangleF> _normalizedRects = [];
	

	public PoolRect Float(IPaintableRect paintable, float left, float top, float width, float height ) {
		_paintables.Add(paintable);
		_normalizedRects.Add(new RectangleF(left,top,width,height));
		return this;
	}

	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		for(int i=0;i< _paintables.Count; ++i) {
			var normalized = _normalizedRects[i];
			var r = new Rectangle(
				rect.X + (int)(rect.Width * normalized.X),
				rect.Y + (int)(rect.Height * normalized.Y),
				(int)(rect.Width * normalized.Width),
				(int)(rect.Height * normalized.Height)
			);
			_paintables[i].Paint(graphics, r );
		}
		return rect;
	}
}
