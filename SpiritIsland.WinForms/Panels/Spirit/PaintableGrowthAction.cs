using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Wraps other paintables, giving them a fixed height to width ratio.
/// Also, provides event to update action screen locations.
/// </summary>
public class PaintableGrowthAction( SpiritGrowthAction action, SharedCtx ctx ) 
	: IPaintableRect
	, IAmEnablable
	, IClickableLocation
	, IPaintAbove
{

	public float? WidthRatio =>.6f;


	public SpiritGrowthAction Action => action;
	public Rectangle Bounds { get; private set; }

	public void Paint( Graphics graphics, Rectangle bounds ) {
		Bounds = bounds.FitBoth(WidthRatio.Value);
		_inner.Paint(graphics, Bounds );
	}

	public bool Enabled { private get; set; }
	void IPaintAbove.PaintAbove( Graphics graphics ) {
		if(Enabled) {
			using Pen highlightPen = new( Color.Red, 4f );
			graphics.DrawRectangle( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	public bool Contains( Point point ) => Bounds.Contains(point);
	public void Click(){
		ctx.SelectOption(action);
	}

	readonly IPaintableRect _inner = PaintableActionFactory.GetGrowthPaintable( action.Cmd );
}