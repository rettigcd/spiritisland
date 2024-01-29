namespace SpiritIsland;

class ColumnRect1 : RowRect {
	protected override Rectangle Flip( Rectangle r ) => new Rectangle(r.Y,r.X,r.Height,r.Width);
	protected override Point Flip( Point p ) => new Point(p.Y,p.X);
}

/// <summary>
/// "Inline" child elements all have the same Width.
/// </summary>
public class ColumnRect : BasePaintableRect {

	/// <summary> Draws lines between cells </summary>
	public PenSpec? Between {get;set;}

	#region constructors

	public ColumnRect( params IPaintableRect[] children ) : this(Align.Near,children){}

	public ColumnRect( Align align, params IPaintableRect[] children ){
		_align = align;
		_children = children;
		if( _children.All(x=>x.WidthRatio.HasValue) )
			WidthRatio = 1 / _children.Sum( x=>1/x!.WidthRatio );
	}

	#endregion constructors

	protected override void PaintContent( Graphics graphics, Rectangle content ){
		switch(_align) {
			case Align.Near: StackNear( graphics, content ); break;
			default: StackFar( graphics, content ); break;
		};
	}

	void StackNear( Graphics graphics, Rectangle content ){
		int nullHeight = CalcNullHeight(content);
		int top = content.Top;
		int index=0;
		foreach(var child in _children){
			int height = child.WidthRatio.HasValue ? (int)(content.Width / child.WidthRatio.Value +.5f) : nullHeight;
			int bottom = top + height;
			var rect = new Rectangle(content.Left,top,content.Width,height);
			PaintChild(child,graphics,rect,index++);
			top = bottom;
		}
	}

	void StackFar( Graphics graphics, Rectangle content ){
		int nullHeight = CalcNullHeight(content);
		int bottom = content.Bottom;
		int index=0;
		foreach(var child in _children){
			int height = child.WidthRatio.HasValue ? (int)(content.Width / child.WidthRatio.Value +.5f) : nullHeight;
			int top = bottom - height;
			var rect = new Rectangle(content.Left,top,content.Width,height);
			PaintChild(child,graphics,rect,index++);
			bottom = top;
		}
	}

	void PaintChild(IPaintableRect child, Graphics graphics, Rectangle bounds, int index){
		child.Paint(graphics,bounds);
		if(index != 0){
			Between?.Stroke(graphics,
				_align == Align.Near ? bounds.TL() : bounds.BL(),
				_align == Align.Near ? bounds.TR() : bounds.BR(),
				bounds
			);
		}
	}

	int CalcNullHeight(Rectangle content){
		int numMissingWidths = _children.Count(x=>x.WidthRatio is null);
		if(numMissingWidths == 0) return 0;
		float usedHeightRatio = 1/_children.Sum(x=>x.WidthRatio .HasValue ? 1/x.WidthRatio.Value : 0);
		int remainingHeight = content.Height - (int)(content.Width / usedHeightRatio);
		if(remainingHeight < 0) return 0;
		return remainingHeight / numMissingWidths;
	}

	readonly Align _align;
	readonly IPaintableRect[] _children;

}
