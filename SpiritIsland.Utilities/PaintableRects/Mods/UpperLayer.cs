namespace SpiritIsland;

// Painting "above" is really a WinForms thing, not a painting/utils thing

public interface IPaintAbove { void PaintAbove( Graphics graphics );  }

public class UpperLayer( IPaintableRect above ) : IPaintAbove, IPaintableRect {
	public float? WidthRatio => _above.WidthRatio;

	public void Paint( Graphics graphics, Rectangle bounds ){ _bounds = bounds; }

	void IPaintAbove.PaintAbove( Graphics graphics ) => _above.Paint(graphics,_bounds);
	
	Rectangle _bounds;
	
	readonly IPaintableRect _above = above;
}
