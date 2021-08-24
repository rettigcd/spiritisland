using System.Drawing;

namespace SpiritIsland.WinForms {

	public static class GraphicsExtensions {


		static public void DrawCount( this Graphics graphics, RectangleF rect, int count )
			=> DrawCount(graphics, ToRect(rect),count);

		static public void DrawCount(this Graphics graphics, Rectangle rect, int count ) {
			if(count > 1) {
				string txt = "x" + count;
				SizeF sz = graphics.MeasureString( txt, countFont );
				var numRect = new RectangleF( rect.Right - sz.Width+2, rect.Bottom - sz.Height, sz.Width + 3, sz.Height + 2 );
				graphics.FillEllipse( Brushes.White, numRect );
				graphics.DrawEllipse( Pens.Black, numRect );
				graphics.DrawString( txt, countFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
			}
		}
		readonly static Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

		static Rectangle ToRect(RectangleF r) => new Rectangle((int)r.X,(int)r.Y,(int)r.Width,(int)r.Height);
	}



}
