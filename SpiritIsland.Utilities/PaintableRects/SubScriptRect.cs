namespace SpiritIsland;

/// <summary>
/// Uses the Bounds as a reference only.  
/// Draws outside of them in bottom right cornern.
/// </summary>
public class SubScriptRect : IPaintableRect {

	readonly static BrushSpec Background = Brushes.White;
	readonly static PenSpec Border = Pens.Black;
	readonly static Brush TextBrush = Brushes.Black;
	readonly static Font _font = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );

	public float? WidthRatio { get; set; }

	#region constructors

	public SubScriptRect(string text){ _text = () => text; }
	public SubScriptRect(Func<string> text){ _text = text; }

	#endregion constructors
	public void Paint( Graphics graphics, Rectangle bounds ){

		string text = _text();
		Size sz = graphics.MeasureString( text, _font ).ToSize();
		var subscriptRect = new Rectangle(
			bounds.Right - sz.Width - (bounds.Width / 8),
			bounds.Bottom - sz.Height,
			sz.Width + 3,
			sz.Height + 2
		);
		Background.FillElipse( graphics, subscriptRect );
		Border.DrawElipse( graphics, subscriptRect );
		graphics.DrawString( text, _font, TextBrush, subscriptRect.X + 2, subscriptRect.Y + 1 );
	}

	readonly Func<string> _text;

}
