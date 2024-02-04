namespace SpiritIsland;

/// <summary>
/// "Inline" child elements all have the same height.
/// </summary>
public class RowRect : BasePaintableRect {

	/// <summary> Draws lines between cells </summary>
	public PenSpec? Between {get;set;}

	#region constructors

	public RowRect( params IPaintableRect[] children ) : this(FillFrom.Left,children){}

	public RowRect( FillFrom align, params IPaintableRect[] children ){
		_align = align;
		_children = children;
		if( _children.All(x=>x.WidthRatio.HasValue))
			WidthRatio = InvertWidthRatio(_children.Sum(c=>InvertWidthRatio(c.WidthRatio!.Value)) );
	}

	#endregion constructors

	protected override void PaintContent( Graphics graphics, Rectangle content ){
		content = Transform(content,true); // hook for columns
		int nullWidth = CalcNullWidth(content);
		int left = content.Left;
		int index=0;
		foreach(var child in _children){
			int width = child.WidthRatio.HasValue ? (int)(content.Height * InvertWidthRatio(child.WidthRatio.Value) +.5f) : nullWidth;
			int right = left + width;
			var childBounds = new Rectangle(left,content.Top,width,content.Height);

			child.Paint(graphics,Transform(childBounds,false));
			PaintInBetweenLine( graphics, child, childBounds, index++ );

			left = right;
		}
	}

	void PaintInBetweenLine( Graphics graphics, IPaintableRect child, Rectangle childBounds, int index ){
		if(index != 0){
			Between?.Stroke(graphics, 
				RestorePoint( childBounds.TL() ), 
				RestorePoint( childBounds.BL() ),
				childBounds
			);
		}
	}

	int CalcNullWidth(Rectangle content){
		int numMissingWidths = _children.Count(x=>x.WidthRatio is null);
		if(numMissingWidths == 0) return 0;
		float usedWidthRatio = _children.Sum(x => x.WidthRatio.HasValue ? InvertWidthRatio(x.WidthRatio.Value) : 0);
		int remainingWidth = content.Width - (int)(content.Height * usedWidthRatio);
		if(remainingWidth < 0) return 0;
		return remainingWidth / numMissingWidths;
	}

	protected Rectangle Transform(Rectangle r, bool makeLeftToRight) => _align switch{
		// Symetrical transformations - ignore makeLeftToRight variable
		FillFrom.Left => r,												// no-change
		FillFrom.Right => new Rectangle(-r.X-r.Width,r.Y,r.Width,r.Height),	// flip over y-axis (negate x)
		FillFrom.Top => new Rectangle(r.Y,r.X,r.Height,r.Width),			// flip over x==y axis (swap x & y)
		// Non-symetrical transformation:
		// makeLeftToRight==true:  Flip_Horizontal(FlipXY(rect))
		// makeLeftToRight==false: FlipXY(Flip_Horizontal(rect))
		FillFrom.Bottom => makeLeftToRight 
			? new Rectangle(-r.Y-r.Height,r.X,r.Height,r.Width) // rotate CCW 90°
			: new Rectangle(r.Y,-r.X-r.Width,r.Height,r.Width), // rotate CW 90°

		_ => throw new InvalidOperationException(),
	};

	protected float InvertWidthRatio(float f) => _align switch{ 
		FillFrom.Left => f,
		FillFrom.Right => f,
		FillFrom.Top => 1/f,
		FillFrom.Bottom => 1/f,
		_ => throw new InvalidOperationException(),
	};

	protected Point RestorePoint(Point p) => _align switch{ 
		FillFrom.Left => p,					// no-change
		FillFrom.Right => new Point(-p.X,p.Y),	// flip over y-axis (negate x)
		FillFrom.Top => new Point(p.Y,p.X),  // flip over (x==y) line (swap x & y)
		FillFrom.Bottom => new Point(p.Y,-p.X),	// rotate CW 90°
		_ => throw new InvalidOperationException(),
	}; // no flipping

	readonly FillFrom _align;
	readonly IPaintableRect[] _children;

}


public enum FillFrom {
	Default,
	/// <summary> Left / Top </summary>
	Left,
	/// <summary> Right / Bottom </summary>
	Right,
	/// <summary> Left / Top </summary>
	Top, // (small Y values)
	/// <summary> Right / Bottom </summary>
	Bottom // (big Y values)

}
