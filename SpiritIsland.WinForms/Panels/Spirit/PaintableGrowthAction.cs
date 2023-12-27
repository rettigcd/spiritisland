using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Wraps other paintables, giving them a fixed height to width ratio.
/// Also, provides event to update action screen locatoins.
/// </summary>
public class PaintableGrowthAction : IPaintableRowMember {

	public float WidthRatio => .6f;

	public PaintableGrowthAction( SpiritGrowthAction action ) {
		Action = action;
		_inner = PaintableActionFactory.GetGrowthPaintable( action.Cmd );
	}

	public SpiritGrowthAction Action { get; }
	public Rectangle Bounds { get; private set; }
	public event Action<PaintableGrowthAction> BoundsChanged;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ) {
		if(Bounds != bounds) {
			Bounds = bounds;
			BoundsChanged?.Invoke(this);
		}
		return _inner.Paint(graphics, bounds );
	}

	readonly IPaintableRect _inner;
}