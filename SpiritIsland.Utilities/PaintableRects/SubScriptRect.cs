using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Uses the Bounds as a reference only.  
/// Draws outside of them in bottom right cornern.
/// </summary>
public class SubScriptRect(string _text) : IPaintableRect {

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){

		Size sz = graphics.MeasureString( _text, _font ).ToSize();
		var subscriptRect = new Rectangle(
			bounds.Right - sz.Width - (bounds.Width / 8),
			bounds.Bottom - sz.Height,
			sz.Width + 3,
			sz.Height + 2
		);
		graphics.FillEllipse( Brushes.White, subscriptRect );
		graphics.DrawEllipse( Pens.Black, subscriptRect );
		graphics.DrawString( _text, _font, Brushes.Black, subscriptRect.X + 2, subscriptRect.Y + 1 );
		return bounds;
	}
	readonly static Font _font = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );	
}
