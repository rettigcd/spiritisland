namespace SpiritIsland;

/// <summary>
/// "Inline" child elements all have the same height.
/// </summary>
public class RowRect : BasePaintableRect {

	/// <summary> Draws lines between cells </summary>
	public PenSpec? Between {get;set;}

	#region constructors

	public RowRect( params IPaintableRect[] children ) : this(Align.Near,children){}

	public RowRect( Align align, params IPaintableRect[] children ){
		_align = align;
		_children = children;
		if( _children.All(x=>x.WidthRatio.HasValue))
			WidthRatio = _children.Sum(c=>c!.WidthRatio );
	}

	#endregion constructors

	protected override void PaintContent( Graphics graphics, Rectangle content ){
		content = Flip(content); // hook for columns
		switch(_align) {
			case Align.Near: StackNear( graphics, content ); break;
			default: StackFar( graphics, content ); break;
		};
	}

	void StackNear( Graphics graphics, Rectangle content ){
		int nullWidth = CalcNullWidth(content);
		int left = content.Left;
		int index=0;
		foreach(var child in _children){
			int width = child.WidthRatio.HasValue ? (int)(content.Height * child.WidthRatio.Value +.5f) : nullWidth;
			int right = left + width;
			var rect = new Rectangle(left,content.Top,width,content.Height);
			PaintChild(child,graphics,rect,index++);
			left = right;
		}
	}

	void StackFar( Graphics graphics, Rectangle content ){
		int nullWidth = CalcNullWidth(content);
		int right = content.Right;
		int index=0;
		foreach(var child in _children){
			int width = child.WidthRatio.HasValue ? (int)(content.Height * child.WidthRatio.Value +.5f) : nullWidth;
			int left = right - width;
			var rect = new Rectangle(left,content.Top,width,content.Height);
			PaintChild(child,graphics,rect,index++);
			right = left;
		}
	}

	void PaintChild(IPaintableRect child, Graphics graphics, Rectangle childBounds, int index){
		child.Paint(graphics,Flip(childBounds));
		if(index != 0){
			Between?.Stroke(graphics,
				_align == Align.Near ? Flip(childBounds.TL()) : Flip(childBounds.TR()),
				_align == Align.Near ? Flip(childBounds.BL()) : Flip(childBounds.BR()),
				childBounds
			);
		}
	}

	int CalcNullWidth(Rectangle content){
		int numMissingWidths = _children.Count(x=>x.WidthRatio is null);
		if(numMissingWidths == 0) return 0;
		float usedWidthRatio = _children.Sum(x=>x.WidthRatio??0);
		int remainingWidth = content.Width - (int)(content.Height * usedWidthRatio);
		if(remainingWidth < 0) return 0;
		return remainingWidth / numMissingWidths;
	}

	virtual protected Rectangle Flip(Rectangle r) => r; // no flipping
	virtual protected Point Flip(Point r) => r; // no flipping

	readonly Align _align;
	readonly IPaintableRect[] _children;

}
