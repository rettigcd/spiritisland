using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class SpaceButton : IButton {

	readonly Func<Space, IVisibleToken, Point> _locationMapper;
	readonly Space _space;
	readonly int _hotSpotRadius;

	public SpaceButton(Func<Space,IVisibleToken,Point> locationMapper, Space space, int hotSpotRadius) {
		_locationMapper = locationMapper;
		_space = space;
		_hotSpotRadius = hotSpotRadius;
	}

	public Rectangle Bounds { 
		get{ 
			var center = _locationMapper(_space,_focusToken);
			return new Rectangle(center.X-_hotSpotRadius, center.Y-_hotSpotRadius, _hotSpotRadius*2, _hotSpotRadius*2 );
		}
	}

	public void Paint( Graphics graphics, bool enabled ) {
		if(enabled) {
			using Pen highlightPen = new( Color.Aquamarine, 5 );
			graphics.DrawEllipse( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	void IButton.SyncDataToDecision( IDecision d ) {
		_focusToken = ((Select.Space)d).Token;
	}
	IVisibleToken _focusToken;
}