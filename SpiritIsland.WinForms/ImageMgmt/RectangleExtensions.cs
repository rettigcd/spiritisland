using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public static class RectangleExtensions {

	#region Split Vertically

	static public Rectangle[] SplitVerticallyAt( this Rectangle rect, params float[] divisions ) {
		int lastY = rect.Y;
		var result = new Rectangle[divisions.Length + 1];
		for(int i = 0; i <= divisions.Length; ++i) {
			int nextY = (i < divisions.Length)
				? rect.Y + (int)(divisions[i] * rect.Height)
				: rect.Bottom;
			result[i] = new Rectangle( rect.X, lastY, rect.Width, nextY - lastY );
			lastY = nextY;
		}
		return result;
	}

	static public Rectangle[] SplitVerticallyByWeight( this Rectangle rect, int rowMargin, params float[] weights ) {
		float total = weights.Sum();
		int workingHeight = rect.Height - (weights.Length - 1) * rowMargin;
		int lastY = rect.Y;
		var result = new Rectangle[weights.Length];
		float current = 0.0f;
		for(int i = 0; i < weights.Length; ++i) {
			current += weights[i] / total;
			int nextY = rect.Y + (int)(current * workingHeight);
			result[i] = new Rectangle( rect.X, lastY + i * rowMargin, rect.Width, nextY - lastY );
			lastY = nextY;
		}
		return result;
	}

	#endregion Split Vertically

	#region Split Horizontally

	static public Rectangle[] SplitHorizontallyByWeight( this Rectangle rect, int rowMargin, params float[] weights ) {
		float total = weights.Sum();
		int workingWidth = rect.Width - (weights.Length - 1) * rowMargin;
		int lastX = rect.X;
		var result = new Rectangle[weights.Length];
		float current = 0.0f;
		for(int i = 0; i < weights.Length; ++i) {
			current += weights[i] / total;
			int nextX = rect.X + (int)(current * workingWidth);
			result[i] = new Rectangle( lastX + i * rowMargin, rect.Y, nextX - lastX, rect.Height );
			lastX = nextX;
		}
		return result;
	}

	static public Rectangle[] SplitHorizontallyRelativeToHeight( this Rectangle rect, int spacer, Align align, params float[] widthsPerHeight ) {

		// check if we have more widths requested, than we can accommodate. 
		// if so, shouldn't we do something about it???
		// bool tooWide = rect.Width < widthsPerHeight.Sum() * rect.Height;

		var result = new Rectangle[widthsPerHeight.Length];
		int x = rect.X;
		for(int i = 0; i < widthsPerHeight.Length; ++i) {
			int width = (int)(widthsPerHeight[i] * rect.Height);
			result[i] = new Rectangle(x,rect.Y,width,rect.Height);
			x += (width + spacer);
		}
		x-=spacer;

		// adjust if right-aligning
		int xAdjustment = align switch {
			Align.Late => rect.Right - x,
			Align.Center => (rect.Right - x)/2,
			_ => 0,
		};
		for(int i = 0; i < result.Length; ++i)
			result[i].X += xAdjustment;

		return result;
	}


	static public Rectangle[] SplitHorizontallyIntoColumns( this Rectangle rect, int rowMargin, int columns ) {
		float columnWidth = 1.0f / columns;
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

	#endregion Split Horizontally

	#region Integers - InflateBy, Fit

	/// <returns>a new rectangle inflated by # of pixels on each side</returns>
	static public Rectangle InflateBy( this Rectangle rect, int delta ) {
		int d2 = delta * 2;
		return new Rectangle( rect.X - delta, rect.Y - delta, rect.Width + d2, rect.Height + d2 );
	}

	static public Rectangle InflateBy( this Rectangle rect, int deltaX, int deltaY )
		=> new Rectangle( rect.X - deltaX, rect.Y - deltaY, rect.Width + deltaX * 2, rect.Height + deltaY * 2 );

	/// <param name="bounds">Rectangle we are trying to fit inside</param>
	/// <param name="size">aspect ratio of final rectangle</param>
	/// <returns>a rectangle center on bounds, with the same height as bounds, with a same width/hieght ratio as size</returns>
	static public Rectangle FitHeight( this Rectangle bounds, Size size, Align align = default ) {
		// Centered Horizontally
		int width = bounds.Height * size.Width / size.Height;
		return align switch {
			Align.Early => new Rectangle( bounds.X, bounds.Y, width, bounds.Height ),
			Align.Late => new Rectangle( bounds.Right - width, bounds.Y, width, bounds.Height ),
			_ => new Rectangle( bounds.X + (bounds.Width - width) / 2, bounds.Y, width, bounds.Height ), // center / default
		};
	}

	public static Rectangle FitWidth( this Rectangle bounds, Size size, Align align = default ) {
		int height = bounds.Width * size.Height / size.Width;
		return align switch {
			Align.Early => new Rectangle( bounds.X, bounds.Y, bounds.Width, height ),
			Align.Late => new Rectangle( bounds.X, bounds.Bottom - height, bounds.Width, height ),
			_ => new Rectangle( bounds.X, bounds.Y + (bounds.Height - height) / 2, bounds.Width, height ), // center / default
		};
	}

	static public Rectangle FitBoth( this Rectangle bounds, Size size, Align horizontal = default, Align vertical = default ) {
		return bounds.Height * size.Width < size.Height * bounds.Width
			? bounds.FitHeight( size, horizontal )
			: bounds.FitWidth( size, vertical );
	}

	#endregion

}
