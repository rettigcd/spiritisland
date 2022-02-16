using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	static public class GraphicsExtensions {

		#region Super-script - Subscript text

		readonly static Font superSubScriptFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

		static public void DrawCountIfHigherThan( this Graphics graphics, RectangleF rect, int count, int highestHidden = 1 ) {
			if(count > highestHidden)
				DrawSubscript(graphics, ToInts(rect), "x" + count);
		}

		static public void DrawSubscript(this Graphics graphics, Rectangle rect, string txt ) {
			SizeF sz = graphics.MeasureString( txt, superSubScriptFont );
			var numRect = new RectangleF( rect.Right - sz.Width+2, rect.Bottom - sz.Height, sz.Width + 3, sz.Height + 2 );
			graphics.FillEllipse( Brushes.White, numRect );
			graphics.DrawEllipse( Pens.Black, numRect );
			graphics.DrawString( txt, superSubScriptFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
		}

		static public void DrawSuperscript( this Graphics graphics, Rectangle rect, string txt ) {
			SizeF sz = graphics.MeasureString( txt, superSubScriptFont );
			var numRect = new RectangleF( rect.Right - sz.Width + 2, rect.Top - sz.Height, sz.Width + 3, sz.Height + 2 );
			graphics.FillEllipse( Brushes.White, numRect );
			graphics.DrawEllipse( Pens.Black, numRect );
			graphics.DrawString( txt, superSubScriptFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
		}

		#endregion

		static public void DrawInvaderCard( this Graphics graphics, RectangleF rect, IInvaderCard card ) {
			if(card==null) return;
			using var img = ResourceImages.Singleton.GetInvaderCard(card.Text);
			graphics.DrawImage(img,rect);
		}

		static public Rectangle ToInts(this RectangleF r) => new Rectangle((int)r.X,(int)r.Y,(int)r.Width,(int)r.Height);
		static public RectangleF ToRectF( this Rectangle r ) => new RectangleF( r.X, r.Y, r.Width, r.Height );

		static public RectangleF Scale( this RectangleF src, float scale ) => new RectangleF( src.X * scale, src.Y * scale, src.Width * scale, src.Height * scale );
		static public SizeF Scale( this SizeF src, float scale ) => new SizeF( src.Width * scale, src.Height * scale );
		static public RectangleF Translate( this RectangleF src, float deltaX, float deltaY ) => new RectangleF( src.X +deltaX, src.Y +deltaY, src.Width, src.Height );

		static public void DrawImageFitHeight( this Graphics graphics, Image image, RectangleF bounds ) {
			graphics.DrawImage( image, bounds.FitHeight( image.Size ).ToInts() );
		}

		static public void DrawImageFitWidth( this Graphics graphics, Image image, RectangleF bounds ) {
			graphics.DrawImage( image, bounds.FitWidth( image.Size ).ToInts() );
		}

		static public void DrawImageFitBoth( this Graphics graphics, Image image, RectangleF bounds ) {
			graphics.DrawImage( image, bounds.FitBoth( image.Size ).ToInts() );
		}


		#region Integers

		/// <summary>
		/// Returns a new rectangle inflated by # of pixels on each side
		/// </summary>
		static public Rectangle InflateBy(this Rectangle rect, int delta ) {
			int d2 = delta*2;
			return new Rectangle( rect.X-delta, rect.Y-delta, rect.Width+d2, rect.Height+d2);
		}

		static public Rectangle[] SplitVerticallyAt( this Rectangle rect, params float[] divisions) {
			int lastY = rect.Y;
			var result = new Rectangle[ divisions.Length+1 ];
			for(int i = 0; i <= divisions.Length; ++i) {
				int nextY = (i < divisions.Length)
					? rect.Y + (int)(divisions[i] * rect.Height)
					: rect.Bottom;
				result[i] = new Rectangle( rect.X, lastY, rect.Width, nextY-lastY );
				lastY = nextY;
			}
			return result;
		}

		static public Rectangle[] SplitVerticallyAt( this Rectangle rect, int rowMargin, params float[] divisions) {
			int workingHeight = rect.Height - divisions.Length * rowMargin;
			int lastY = rect.Y;
			var result = new Rectangle[ divisions.Length+1 ];
			for(int i = 0; i <= divisions.Length; ++i) {
				int nextY = (i < divisions.Length)
					? rect.Y + (int)(divisions[i] * workingHeight)
					: rect.Bottom;
				result[i] = new Rectangle( rect.X, lastY + i*rowMargin, rect.Width, nextY-lastY );
				lastY = nextY;
			}
			return result;
		}

		static public Rectangle[] SplitVerticallyByWeight( this Rectangle rect, int rowMargin, params float[] weights) {
			float total = weights.Sum();
			int workingHeight = rect.Height - (weights.Length-1) * rowMargin;
			int lastY = rect.Y;
			var result = new Rectangle[ weights.Length ];
			float current = 0.0f;
			for(int i = 0; i < weights.Length; ++i) {
				current += weights[i]/total;
				int nextY = rect.Y + (int)(current * workingHeight);
				result[i] = new Rectangle( rect.X, lastY + i*rowMargin, rect.Width, nextY-lastY );
				lastY = nextY;
			}
			return result;
		}

		static public Rectangle[] SplitHorizontallyByWeight( this Rectangle rect, int rowMargin, params float[] weights) {
			float total = weights.Sum();
			int workingWidth = rect.Width - (weights.Length-1) * rowMargin;
			int lastX = rect.X;
			var result = new Rectangle[ weights.Length ];
			float current = 0.0f;
			for(int i = 0; i < weights.Length; ++i) {
				current += weights[i]/total;
				int nextX = rect.X + (int)(current * workingWidth);
				result[i] = new Rectangle( lastX + i*rowMargin, rect.Y, nextX-lastX, rect.Height );
				lastX = nextX;
			}
			return result;
		}

		static public Rectangle[] SplitHorizontallyIntoColumns( this Rectangle rect, int rowMargin, int columns ) {
			float columnWidth = 1.0f/columns;
			int workingWidth = rect.Width - (columns - 1) * rowMargin;
			int lastX = rect.X;
			var result = new Rectangle[columns];
			float current = 0.0f;
			for(int i = 0; i < columns; ++i) {
				current += columnWidth;
				int nextX = rect.X + (int)(current * workingWidth);
				result[i] = new Rectangle( lastX + i * rowMargin, rect.Y, nextX - lastX, rect.Height );
				lastX = nextX;
			}
			return result;
		}

		static public Rectangle[] SplitHorizontally( this Rectangle rect, int divisions ) {
			var result = new Rectangle[divisions];
			int lastX = 0;
			for(int i = 0; i < divisions; ++i) {
				int nextX = rect.Width * (i + 1) / divisions;
				result[i] = new Rectangle( rect.X + lastX, rect.Y, nextX - lastX, rect.Height );
				lastX = nextX;
			}
			return result;
		}


		/// <param name="bounds">Rectangle we are trying to fit inside</param>
		/// <param name="size">aspect ratio of final rectangle</param>
		/// <returns>a rectangle center on bounds, with the same height as bounds, with a same width/hieght ratio as size</returns>
		static public Rectangle FitHeight( this Rectangle bounds, Size size ) {
			// Centered Horizontally
			int width = bounds.Height * size.Width / size.Height;
			return new Rectangle( bounds.X + (bounds.Width - width) / 2, bounds.Y, width, bounds.Height );
		}

		public static Rectangle FitWidth( this Rectangle bounds, Size size ) {
			int height = bounds.Width * size.Height / size.Width;
			// Centered Horizontally
			return new Rectangle( bounds.X, bounds.Y + (bounds.Height - height) / 2, bounds.Width, height );
		}

		static public Rectangle FitBoth( this Rectangle bounds, int width, int height ) => bounds.FitBoth(new Size(width,height));
		static public Rectangle FitBoth( this Rectangle bounds, Size size ) {
			// if normalized image height is greater than bounds, then fit height
			return size.Height*bounds.Width > bounds.Height*size.Width 
				? bounds.FitHeight(size) 
				: bounds.FitWidth(size);
		}

		#endregion

		#region Float

		static public RectangleF[] SplitVerticallyAt( this RectangleF rect, params float[] splitPercentages) {
			var ys = new List<float>() { rect.Y };
			ys.AddRange(splitPercentages.Select(p=>rect.Y+rect.Height*p));
			ys.Add( rect.Bottom );
			var result = new RectangleF[ splitPercentages.Length+1 ];
			for(int i=0;i<ys.Count-1;++i)
				result[i] = new RectangleF( rect.X ,ys[i], rect.Width, ys[i+1]-ys[i]);
			return result;
		}

		/// <summary>
		/// Returns a new rectangle inflated by # of pixels on each side
		/// </summary>
		static public RectangleF InflateBy(this RectangleF rect, float delta ) {
			float d2 = delta*2;
			return new RectangleF( rect.X-delta, rect.Y-delta, rect.Width+d2, rect.Height+d2);
		}

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


		/// <param name="bounds">Rectangle we are trying to fit inside</param>
		/// <param name="size">aspect ratio of final rectangle</param>
		/// <returns>a rectangle center on bounds, with the same height as bounds, with a same width/hieght ratio as size</returns>
		static public RectangleF FitHeight( this RectangleF bounds, SizeF size ) {
			// Centered Horizontally
			float width = bounds.Height * size.Width / size.Height;
			return new RectangleF( bounds.X + (bounds.Width - width) / 2, bounds.Y, width, bounds.Height );
		}

		public static RectangleF FitWidth( this RectangleF bounds, SizeF size ) {
			float height = bounds.Width * size.Height / size.Width;
			// Centered Horizontally
			return new RectangleF( bounds.X, bounds.Y + (bounds.Height - height) / 2, bounds.Width, height );
		}

		static public RectangleF FitBoth( this RectangleF bounds, float width, float height ) => bounds.FitBoth(new SizeF(width,height));
		static public RectangleF FitBoth( this RectangleF bounds, SizeF size ) {
			// if normalized image height is greater than bounds, then fit height
			return size.Height*bounds.Width > bounds.Height*size.Width 
				? bounds.FitHeight(size) 
				: bounds.FitWidth(size);
		}

		#endregion

	}



}
