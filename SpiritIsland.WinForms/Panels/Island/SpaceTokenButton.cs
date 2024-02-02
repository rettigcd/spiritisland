using System.Drawing;

namespace SpiritIsland.WinForms;

public class SpaceTokenButton( Rectangle bounds ) : IButton {
	public Rectangle Bounds { get; private set; } = bounds;
	bool IButton.Contains( Point clientCoords) => Bounds.Contains( clientCoords );

	public bool Enabled{ private get; set; }
	public void PaintAbove( Graphics graphics ) {
		if(Enabled) {
			using Pen highlightPen = new( Color.Aquamarine, 5 );
			graphics.DrawRectangle( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

}