using System.Drawing;

namespace SpiritIsland;

// Holds 1 or more cells.
// Cells can be sized to either:
//		(a) have same height or
//		(b) have same width (is this even useful???)
// Hight can be Collapsed Top / Middle / Bottom   OR Expand-Fill
public class PaintableRow : IPaintableRowMember {

	public float WidthRatio { get; private set; }

	#region Style
	public Color? BackgroundColor { get; set; }
	public Color? BorderColor { get; set; }
	/// <summary> (% of height) to add around the border (using height because it is constant, width is variable</summary>
	public float Padding { get; set; }
	/// <summary> (% of height) to space between children (using height because it is constant, width is variable</summary>
	public float Separation { get; set; }
	#endregion  Style

	public PaintableRow( params IPaintableRowMember[] children ) {
		_children = children;
		_rects = new Rectangle[_children.Length];

		// calculate static right-weights - using All-Have-Height=1 and Expand-to-fill-Width
		_rights = new float[children.Length];
		float cur = 0;
		for(int i = 0; i < children.Length; ++i) {
			cur += children[i].WidthRatio;
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

	public Rectangle Paint( Graphics graphics, Rectangle bounds ) {
		PaintStyles( graphics, bounds );

		var rects = CalcChildRects( bounds );

		// Draw Children
		for(int i = 0; i < _children.Length; ++i)
			_children[i].Paint( graphics, rects[i] );

		return bounds;
	}

	void PaintStyles( Graphics graphics, Rectangle bounds ) {
		if(BackgroundColor.HasValue) {
			using var growthGroupBrush = new SolidBrush( BackgroundColor.Value );
			graphics.FillRectangle( growthGroupBrush, bounds );
		}
		if(BorderColor.HasValue) {
			using var pen = new Pen( BorderColor.Value );
			graphics.DrawRectangle( pen, bounds );
		}
	}

	/// <remarks>Returning the rectangles isolates caller from knowing if they are cached or calculated on the fly.</remarks>
	Rectangle[] CalcChildRects( Rectangle bounds ) {
		if(bounds == _lastBounds) return _rects;

		// Note: If caller uses WidthRatio to properly size this BEFORE this is called,
		// this will fit perfectly and there will be no left over space that we need to deal with by setting alignment.

		// padding
		int padding = (int)(bounds.Height * Padding + .5f);
		Rectangle innerBounds = bounds.InflateBy( -padding );

		// child separation
		int separation = (int)(bounds.Height * Separation + .5f);
		int usableWidth = innerBounds.Width - separation * (_children.Length - 1);

		int left0 = 0; // 0 origin
		int leftRef = innerBounds.Left;
		for(int i = 0; i < _children.Length; ++i) {
			int right0 = (int)(usableWidth * _rights[i]);
			_rects[i] = new Rectangle( leftRef + left0, innerBounds.Top, right0 - left0, innerBounds.Height );
			left0 = right0;
			leftRef += separation;
		}
		_lastBounds = bounds;
		return _rects;
	}

	#region private fields
	readonly IPaintableRect[] _children;
	readonly Rectangle[] _rects;
	readonly float[] _rights;

	Rectangle _lastBounds;
	#endregion

}
