using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class SpaceButton : IButton {

	readonly Func<Space, IVisibleToken, Point> _locationMapper;
	readonly Space _space;
	readonly int _hotSpotRadius;
//	int TokenSize { get; set; }

	public SpaceButton( Func<Space, IVisibleToken, Point> locationMapper, Space space, int hotSpotRadius ) {
		_locationMapper = locationMapper;
		_space = space;
		_hotSpotRadius = hotSpotRadius;
//		TokenSize = hotSpotRadius * 2;
	}

	public Rectangle Bounds {
		get {
			var center = _locationMapper( _space, _focusToken );
			int radius = (int)(_hotSpotRadius * (_focusToken != null ? 1f : 1.5f)); // make space selection circles a little larger
			return new Rectangle( center.X - radius, center.Y - radius, radius * 2, radius * 2 );
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