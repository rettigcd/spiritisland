using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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


		static public RectangleF[] SplitHorizontally( this RectangleF rect, params float[] splitPercentages) {
			var xs = new List<float>() { rect.X };
			xs.AddRange(splitPercentages.Select(p=>rect.X+rect.Width*p));
			xs.Add( rect.Right );
			var result = new RectangleF[ splitPercentages.Length+1 ];
			for(int i=0;i<xs.Count-1;++i)
				result[i] = new RectangleF(xs[i],rect.Y,xs[i+1]-xs[i],rect.Height);
			return result;
		}

		static public RectangleF[] SplitHorizontally( this RectangleF rect, int divisions) {
			var result = new RectangleF[ divisions ];
			float width = rect.Width/divisions;
			for(int i=0;i<divisions;++i)
				result[i] = new RectangleF( rect.X+i*width, rect.Y, width, rect.Height);
			return result;
		}

		static public RectangleF[] SplitVertically( this RectangleF rect, int divisions) {
			var result = new RectangleF[ divisions ];
			float height = rect.Height/divisions;
			for(int i=0;i<divisions;++i)
				result[i] = new RectangleF(rect.X,rect.Y+i*height,rect.Width,height);
			return result;
		}

		static public RectangleF[] SplitVertically( this RectangleF rect, params float[] splitPercentages) {
			var ys = new List<float>() { rect.Y };
			ys.AddRange(splitPercentages.Select(p=>rect.Y+rect.Height*p));
			ys.Add( rect.Bottom );
			var result = new RectangleF[ splitPercentages.Length+1 ];
			for(int i=0;i<ys.Count-1;++i)
				result[i] = new RectangleF( rect.X ,ys[i], rect.Width, ys[i+1]-ys[i]);
			return result;
		}

		static public void DrawImageFitHeight( this Graphics graphics, Image image, RectangleF bounds ) {
			float width = bounds.Height * image.Width / image.Height;
			// Centered Horizontally
			graphics.DrawImage(image,bounds.X + (bounds.Width-width)/2,bounds.Y,width,bounds.Height);
		}

		static public void DrawImageFitWidth( this Graphics graphics, Image image, RectangleF bounds ) {
			float height = bounds.Width * image.Height / image.Width;
			// Centered Horizontally
			graphics.DrawImage(image,bounds.X, bounds.Y+ (bounds.Height-height)/2,bounds.Width,height);
		}

		static public void DrawImageFitBoth( this Graphics graphics, Image image, RectangleF bounds ) {
			// if normalized image height is greater than bounds, then fit height
			if(image.Height*bounds.Width > bounds.Height*image.Width)
				graphics.DrawImageFitHeight(image,bounds);
			else
				graphics.DrawImageFitWidth(image,bounds);
		}


		static public RectangleF InflateBy(this RectangleF rect, float delta ) {
			float d2 = delta*2;
			return new RectangleF( rect.X-delta, rect.Y-delta, rect.Width+d2, rect.Height+d2);
		}

		//static public void DrawString(this Graphics graphics, string text, Font font, Brush brush,  ) {
		//				graphics.DrawString("some string", font, Brushes.Black, PositionInside(boundingRect,Left,Middle)

		//}

	}



}
