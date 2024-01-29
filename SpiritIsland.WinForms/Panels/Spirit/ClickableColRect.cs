using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Each Innate is naturally arranged as a Column (aka ColumnRect)
/// This extends that and allows us to click on it as an IOption
/// </summary>
class ClickableColRect
	: ColumnRect
	, IPaintAbove
	, IClickableLocation
	, IAmEnablable
{

	public ClickableColRect( params IPaintableRect[] children ):base(children){}

	Rectangle _bounds;

	public override void Paint(Graphics graphics, Rectangle bounds) {
		_bounds = bounds;
		base.Paint(graphics, bounds);
	}

	public bool Enabled { private get; set; }

	public void Click() => Clicked?.Invoke();

	public event Action Clicked;

	public bool Contains(Point point) => _bounds.Contains(point);

	public void PaintAbove(Graphics graphics) {
		if(Enabled)
			((PenSpec)"Red;.02").Stroke(graphics,_bounds);
	}

}

