using System.Drawing;

namespace SpiritIsland;

public class TextRect : IPaintableRect {
	public TextRect( string text ) { _text = text; }
	public TextRect( object obj ) { _text = obj.ToString() ?? string.Empty; }
	readonly string _text;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		Font font = ResourceImages.Singleton.UseGameFont( rect.Height );
		try {
			SizeF textSize = graphics.MeasureString(_text,font);
			Rectangle fitted = rect.FitHeight( textSize.ToSize() );

			if(rect.Width < fitted.Width ) { // too narrow
				font.Dispose();
				// Scale down font
				font = ResourceImages.Singleton.UseGameFont( rect.Height * rect.Width / fitted.Width *.9f );
				textSize = graphics.MeasureString( _text, font );
				fitted = rect.FitWidth( textSize.ToSize() );
			}

			using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			graphics.DrawString( _text, font, Brushes.Black, rect, alignCenter ); // using rect instead of fitted because fitted sometimes crops

			return fitted;
		}
		finally {
			font?.Dispose();
		}
	}

}
