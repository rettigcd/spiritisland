using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Each Innate is naturally arranged as a Column
/// This extends that and allows us to click on it as an IOption
/// </summary>
class ClickableColRect( params IPaintableRect[] children )
	: RowRect( FillFrom.Top, children )
	, IPaintAbove
	, IClickableLocation
	, IAmEnablable
{
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
			((PenSpec)"Red;.02").DrawRectangle(graphics,_bounds);
	}

}

