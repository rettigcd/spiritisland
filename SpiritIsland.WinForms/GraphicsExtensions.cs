using System.Drawing;

namespace SpiritIsland.WinForms {

	static public class GraphicsExtensions {


		static public void DrawCount( this Graphics graphics, RectangleF rect, int count )
			=> DrawSubscript(graphics, ToInts(rect),count);

		static public void DrawSubscript(this Graphics graphics, Rectangle rect, int count ) {
			if(count > 1) {
				string txt = "x" + count;
				SizeF sz = graphics.MeasureString( txt, countFont );
				var numRect = new RectangleF( rect.Right - sz.Width+2, rect.Bottom - sz.Height, sz.Width + 3, sz.Height + 2 );
				graphics.FillEllipse( Brushes.White, numRect );
				graphics.DrawEllipse( Pens.Black, numRect );
				graphics.DrawString( txt, countFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
			}
		}

		static public void DrawSuperscript( this Graphics graphics, Rectangle rect, int count ) {
			if(count > 1) {
				string txt = "x" + count;
				SizeF sz = graphics.MeasureString( txt, countFont );
				var numRect = new RectangleF( rect.Right - sz.Width + 2, rect.Top - sz.Height, sz.Width + 3, sz.Height + 2 );
				graphics.FillEllipse( Brushes.White, numRect );
				graphics.DrawEllipse( Pens.Black, numRect );
				graphics.DrawString( txt, countFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
			}
		}


		readonly static Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

		static public void DrawInvaderCard( this Graphics graphics, RectangleF rect, InvaderCard card ) {
			if(card==null) return;
			using var img = ResourceImages.Singleton.GetInvaderCard(card.Text);
			graphics.DrawImage(img,rect);
		}

		static public void DrawFearCard( this Graphics graphics, RectangleF rect, DisplayFearCard displayFearCard ) {
			if(displayFearCard==null) return;
			using var img = new FearCardImageManager().GetImage( displayFearCard );
			graphics.DrawImage( img, rect );
		}


		static public Rectangle ToInts(this RectangleF r) => new Rectangle((int)r.X,(int)r.Y,(int)r.Width,(int)r.Height);
		static public RectangleF ToRectF( this Rectangle r ) => new RectangleF( r.X, r.Y, r.Width, r.Height );

		static public RectangleF Scale( this RectangleF src, float scale ) => new RectangleF( src.X * scale, src.Y * scale, src.Width * scale, src.Height * scale );
		static public SizeF Scale( this SizeF src, float scale ) => new SizeF( src.Width * scale, src.Height * scale );
		static public RectangleF Translate( this RectangleF src, float deltaX, float deltaY ) => new RectangleF( src.X +deltaX, src.Y +deltaY, src.Width, src.Height );

	}



}
