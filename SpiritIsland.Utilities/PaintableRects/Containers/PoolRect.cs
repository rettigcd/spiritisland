namespace SpiritIsland;

/// <summary>
/// "Pool" - Holds "floating" rects
/// </summary>
public class PoolRect : IPaintableRect {

	/// <remarks>
	/// Don't set this to 1.  For including in rows with equal distribution, it needs to to be null and it is screwing to default it to 1 (which is meaningless) and then clear it.
	/// Instead, leave it null, and set it to something when needed.
	/// See: target column in A Dreadful Tide of Scurrying Flesh
	/// </remarks>
	public float? WidthRatio {get; set;}

	public BrushSpec? Background {get;set;}
	public PenSpec? Border {get;set;}

	// No Padding because that would be confusing and be dupliated with the float parameters
	public PaddingSpec Margin = PaddingSpec.None;

	/// <summary> Captures the location passed to Paint(...,bounds); </summary>
	public Rectangle Bounds {get; private set;}

	public PoolRect Float(IPaintableRect paintable ) => Float(paintable,0,0,100,100);

	public IPaintableRect Child(int index) => _floats[index].child;

	// specifies floating position with integer % (0..100)
	public PoolRect Float(IPaintableRect paintable, int x, int y, int w, int h, Align horizontal=Align.Default, Align vertical=Align.Default ) 
		=> Float( paintable, .01f*x, .01f*y, .01f*w, .01f*h, horizontal, vertical );

	public PoolRect Float(IPaintableRect paintable, float x, float y, float w, float h, Align horizontal=Align.Default, Align vertical=Align.Default ) {
		_floats.Add( new FloatDetails( paintable, new RectangleF(x,y,w,h), horizontal, vertical ) );
		return this;
	}

	public void Paint( Graphics graphics, Rectangle bounds ) {
		Bounds = Margin.Content(bounds);		
		Background?.Fill( graphics, Bounds );
		PaintChildren( graphics, Bounds );
		Border?.Stroke( graphics, Bounds ); // doing last to clean-up edges
	}

	void PaintChildren( Graphics graphics, Rectangle content ) {
		foreach( var floatee in _floats )
			floatee.Paint( graphics, content );
	}

	readonly List<FloatDetails> _floats = [];

	record FloatDetails(IPaintableRect child, RectangleF norm, Align horizontal, Align vertical){
		public void Paint( Graphics graphics, Rectangle bounds ){
			var content = new Rectangle(
				bounds.X + (int)(bounds.Width * norm.X),
				bounds.Y + (int)(bounds.Height * norm.Y),
				(int)(bounds.Width * norm.Width),
				(int)(bounds.Height * norm.Height)
			);
			// if(horizontal != Align.Default){
			// }
			if(child.WidthRatio.HasValue)
				content = content.FitBoth(child.WidthRatio.Value,horizontal,vertical);
			child.Paint( graphics, content );
		}
	}

}
