using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Wraps a IPaintableBlockRect, tracks location, and options
/// </summary>
/// <example> CommandBeasts </example>
class ClickableLocation( IPaintableRect child, Action action ) : IPaintableRect, IClickableLocation {
	public float? WidthRatio { get; set; }

	readonly IPaintableRect _child = child;

	public Rectangle Bounds { get; private set; }
	public bool Contains( Point point ) => Bounds.Contains(point);

	public void Paint( Graphics graphics, Rectangle bounds ) {
		if(WidthRatio.HasValue)
			bounds = bounds.FitBoth(WidthRatio.Value);
		Bounds = bounds;
		_child.Paint(graphics,Bounds);
	}

	public void Click() { _action(); }

	readonly Action _action = action;

}
