using System.Drawing;

namespace SpiritIsland.WinForms;

public class SpaceTokenButton : IButton {

	public SpaceTokenButton( Rectangle bounds ) {
		Bounds = bounds;
	}

	public Rectangle Bounds { get; private set; }

	public void Paint( Graphics graphics, bool enabled ) {
		if(enabled) {
			using Pen highlightPen = new( Color.Aquamarine, 5 );
			graphics.DrawRectangle( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	void IButton.SyncDataToDecision( IDecision _ ) {}

}