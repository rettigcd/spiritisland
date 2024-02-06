namespace SpiritIsland;

// Holds 1 or more cells.
// Cells can be sized to either:
//		(a) have same height or
//		(b) have same width (is this even useful???)
// Hight can be Collapsed Top / Middle / Bottom   OR Expand-Fill
public class RowRect_WithPadding : IPaintableRect {

	public float? WidthRatio { get; private set; }

	#region Style
	public BrushSpec? Background { get; set; }
	public PenSpec? Border { get; set; }

	/// <summary> (% of Min(width,height)) to add around the border</summary>
	public float Padding { get; set; }

	/// <summary> (% of height) to space between children (using height because it is constant, width is variable</summary>
	public float Separation { get; set; }

	#endregion  Style

	#region constructors

	public RowRect_WithPadding( params IPaintableRect[] children ) {
		foreach(var child in children)
			ArgumentNullException.ThrowIfNull(child.WidthRatio);
		_children = children;
		_cachedRects = new Rectangle[_children.Length];

		// calculate static right-weights - using All-Have-Height=1 and Expand-to-fill-Width
		_rights = new float[children.Length];
		float cur = 0;
		for(int i = 0; i < children.Length; ++i) {
			cur += children[i].WidthRatio!.Value;
			_rights[i] = cur;
		}

		float totalWidth = _rights[^1] + Padding * 2 + Separation * (children.Length - 1);
		float totalHeight = (1f + Padding * 2);
		WidthRatio = totalWidth / totalHeight;

		// normalize
		var scale = 1 / _rights[^1];
		for(int i = 0; i < children.Length; ++i)
			_rights[i] *= scale;
		_rights[^1] = 1f; // fix any round off

	}

	#endregion constructors

	public void Paint( Graphics graphics, Rectangle bounds ) {
		Background?.Fill(graphics,bounds);

		var rects = CalcChildRects( bounds );
		// Draw Children
		for(int i = 0; i < _children.Length; ++i)
			_children[i].Paint( graphics, rects[i] );

		Border?.DrawRectangle(graphics,bounds);
	}


	/// <remarks>Returning the rectangles isolates caller from knowing if they are cached or calculated on the fly.</remarks>
	Rectangle[] CalcChildRects( Rectangle bounds ) {
		if(bounds == _lastBounds) return _cachedRects;

		// Note: If caller uses WidthRatio to properly size this BEFORE this is called,
		// this will fit perfectly and there will be no left over space that we need to deal with by setting alignment.

		// padding
		Rectangle innerBounds = bounds.Pad(Padding);

		// child separation
		int separation = (int)(bounds.Height * Separation + .5f);
		int usableWidth = innerBounds.Width - separation * (_children.Length - 1);

		int left0 = 0; // 0 origin
		int leftRef = innerBounds.Left;
		for(int i = 0; i < _children.Length; ++i) {
			int right0 = (int)(usableWidth * _rights[i]);
			_cachedRects[i] = new Rectangle( leftRef + left0, innerBounds.Top, right0 - left0, innerBounds.Height );
			left0 = right0;
			leftRef += separation;
		}
		_lastBounds = bounds;
		return _cachedRects;
	}

	#region private fields
	readonly IPaintableRect[] _children;
	readonly Rectangle[] _cachedRects;
	readonly float[] _rights;

	Rectangle _lastBounds;
	#endregion

}
