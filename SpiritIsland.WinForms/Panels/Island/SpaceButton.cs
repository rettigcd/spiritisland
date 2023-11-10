using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public class SpaceButton : IButton {

	readonly Func<Space, IToken, Point> _locationMapper;
	readonly Space _space;
	readonly int _hotSpotRadius;

	public SpaceButton( Func<Space, IToken, Point> locationMapper, Space space, int hotSpotRadius ) {
		_locationMapper = locationMapper;
		_space = space;
		_hotSpotRadius = hotSpotRadius;
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
		_focusToken = ((A.Space)d).Token;
	}
	IToken _focusToken;
}
