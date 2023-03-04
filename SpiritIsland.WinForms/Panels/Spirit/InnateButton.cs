using System.Drawing;

namespace SpiritIsland.WinForms;

public class InnateButton : IButton {

	public Rectangle Bounds { get; set; }

	public void Paint( Graphics graphics, bool enabled ) {
		if( enabled) {
			using Pen highlightPen = new( Color.Red, 2f );
			graphics.DrawRectangle( highlightPen, Bounds );
		}
	}

	void IButton.SyncDataToDecision( IDecision _ ) { }

}
