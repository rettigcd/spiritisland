namespace SpiritIsland;

/// <summary>
/// "Pool" - Holds "floating" rects
/// </summary>
public class PoolRect : IPaintableRect {

	public float? WidthRatio {get; set;}

	public BrushSpec? Background {get;set;}
	public PenSpec? Border {get;set;}

	// No Padding because that would be confusing and be dupliated with the float parameters
	public PaddingSpec Margin = PaddingSpec.None;

	/// <summary> Captures the location passed to Paint(...,bounds); </summary>
	public Rectangle Bounds {get; private set;}

	public readonly List<Floatable> Children = [];

	#region public Float(...) methods
	public PoolRect Float(IPaintableRect paintable ) => Float(paintable,0,0,100,100);

	// specifies floating position with integer % (0..100)
	public PoolRect Float(IPaintableRect paintable, int x, int y, int w, int h, Align horizontal=Align.Default, Align vertical=Align.Default ) 
		=> Float( paintable, .01f*x, .01f*y, .01f*w, .01f*h, horizontal, vertical );

	public PoolRect Float(IPaintableRect paintable, float x, float y, float w, float h, Align horizontal=Align.Default, Align vertical=Align.Default ) {
		Children.Add( new Floatable( paintable, new RectangleF(x,y,w,h), horizontal, vertical ) );
		return this;
	}

	#endregion public Float(...) methods

	public void Paint( Graphics graphics, Rectangle bounds ) {
		Bounds = Margin.Content(bounds);		
		Background?.Fill( graphics, Bounds );
		PaintChildren( graphics, Bounds );
		Border?.DrawRectangle( graphics, Bounds ); // doing last to clean-up edges
	}

	#region private

	void PaintChildren( Graphics graphics, Rectangle content ) {
		foreach( var floatee in Children )
			floatee.Paint( graphics, content );
	}
	public record Floatable(IPaintableRect Paintable, RectangleF norm, Align horizontal, Align vertical){
		public void Paint( Graphics graphics, Rectangle bounds ){
			var content = new Rectangle(
				bounds.X + (int)(bounds.Width * norm.X),
				bounds.Y + (int)(bounds.Height * norm.Y),
				(int)(bounds.Width * norm.Width),
				(int)(bounds.Height * norm.Height)
			);
			// if(horizontal != Align.Default){
			// }
			if(Paintable.WidthRatio.HasValue)
				content = content.FitBoth(Paintable.WidthRatio.Value,horizontal,vertical);
			Paintable.Paint( graphics, content );
		}
	}

	#endregion private

}
