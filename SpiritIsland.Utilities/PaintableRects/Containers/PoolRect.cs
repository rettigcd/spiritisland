namespace SpiritIsland;

/// <summary>
/// "Pool" - Holds "floating" rects
/// </summary>
public class PoolRect : IPaintableRect {
	readonly List<IPaintableRect> _paintables = [];
	readonly List<RectangleF> _normalizedRects = [];
	
	/// <remarks>
	/// Don't set this to 1.  For including in rows with equal distribution, it needs to to be null and it is screwing to default it to 1 (which is meaningless) and then clear it.
	/// Instead, leave it null, and set it to something when needed.
	/// See: target column in A Dreadful Tide of Scurrying Flesh
	/// </remarks>
	public float? WidthRatio {get; set;}

	public PoolRect Float(IPaintableRect paintable ) => Float(paintable,0,0,100,100);

	// public PoolRect FloatXWYH(IPaintableRect paintable, float x, float width, float y, float height ) => Float(paintable,x,y,width,height);

	// specifies floating position with integer % (0..100)
	public PoolRect Float(IPaintableRect paintable, int x, int y, int w, int h ) 
		=> Float( paintable, .01f*x, .01f*y, .01f*w, .01f*h );

	public PoolRect Float(IPaintableRect paintable, float x, float y, float w, float h ) {
		_paintables.Add(paintable);
		_normalizedRects.Add(new RectangleF(x,y,w,h));
		return this;
	}

	public void Paint( Graphics graphics, Rectangle rect ) {
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
	}
}
