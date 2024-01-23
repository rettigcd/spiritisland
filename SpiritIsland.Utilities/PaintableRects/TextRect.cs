using System.Drawing;

namespace SpiritIsland;

public class TextRect : IPaintableRect {
	public TextRect( string text ) { _text = text; }
	public TextRect( object obj ) { _text = obj.ToString() ?? string.Empty; }
	public float ScaleFont = 1f;
	readonly string _text;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		Font font = ResourceImages.Singleton.UseGameFont( rect.Height * ScaleFont );
		try {
			using StringFormat alignCenter = new StringFormat { 
				Alignment = StringAlignment.Center, 
				LineAlignment = StringAlignment.Center, 
				Trimming = StringTrimming.None,
				FormatFlags = StringFormatFlags.NoWrap,
			};

			SizeF textSize = graphics.MeasureString(_text,font, new PointF(0,0), alignCenter);
//			Rectangle fitted = rect.FitHeight( textSize.ToSize() );

			if(rect.Width < textSize.Width ) { // too narrow
				font.Dispose();
				// Scale down font
				font = ResourceImages.Singleton.UseGameFont( rect.Height * rect.Width / textSize.Width *.9f );
				textSize = graphics.MeasureString( _text, font );
				// fitted = rect.FitWidth( textSize.ToSize() );

			}
	
			graphics.DrawString( _text, font, Brushes.Black, rect, alignCenter ); // using rect instead of fitted because fitted sometimes crops

			return rect; // fitted;
		}
		finally {
			font?.Dispose();
		}
	}

}
