namespace SpiritIsland;

public static class Rectangle_Extensions {

	#region Split Vertically

	static public Rectangle[] SplitVerticallyIntoRows( this Rectangle rect, int rowMargin, int rows ) {
		int heightPlusMargin = rect.Height + rowMargin; // since there are 1 more rows than margins, add a stand-in margin
		int rowAndMarginHeight = heightPlusMargin / rows; // height of 1 row + 1 margin
		int rowHeight = rowAndMarginHeight-rowMargin;
		int remainderHeight = heightPlusMargin - rowAndMarginHeight * rows; // leftover/remaining height due to round-off in row height calculation

		var result = new Rectangle[rows];
		int lastMarginBottom = rect.Y;
		for(int row = 0; row < rows; ++row) {
			result[row] = new Rectangle(rect.X, lastMarginBottom, rect.Width, rowHeight);
			lastMarginBottom += rowAndMarginHeight;
			// Distribute the remainder height evenly across the margins
			if(row<remainderHeight)
				++lastMarginBottom;
		}
		return result;
	}


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
	static public Rectangle[] SplitVerticallyByWeight( this Rectangle rect, float rowMarginPercentage, params float[] weights ) {
		int rowMargin = (int)(rect.Height * rowMarginPercentage);
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


	static public Rectangle[] SplitVerticallyByHeights( this Rectangle rect, params int[] heights ) {
		int tooBig = heights.Sum()-rect.Height;
		if( tooBig != 0) {
			(string dirStr,int size) = 0<tooBig ? ("BIG",tooBig) : ("SMALL",-tooBig);
			throw new ArgumentException( $"Individual heights are too {dirStr} by {size}");
		}
		Rectangle[] res = new Rectangle[heights.Length];
		int y=rect.Y;
		for(int i = 0; i < heights.Length; ++i) {
			int height = heights[i];
			res[i] = new Rectangle( rect.X, y, rect.Width, height );
			y+=height;
		}
		return res;
	}

	#endregion Split Vertically

	#region Split Horizontally

	// !!! transition uses of this over to use marginPercentage
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

	static public Rectangle[] SplitHorizontallyByWeight( this Rectangle rect, float rowMarginPercentage, params float[] weights ) {
		int rowMargin = (int)(rowMarginPercentage * rect.Width);
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
			Align.Far => rect.Right - x,
			Align.Center => (rect.Right - x)/2,
			_ => 0,
		};
		for(int i = 0; i < result.Length; ++i)
			result[i].X += xAdjustment;

		return result;
	}


	static public Rectangle[] SplitHorizontallyIntoColumns( this Rectangle rect, int colMargin, int columns ) {
		float columnWidth = 1.0f / columns;
		int workingWidth = rect.Width - (columns - 1) * colMargin;
		int lastX = rect.X;
		var result = new Rectangle[columns];
		float current = 0.0f;
		for(int i = 0; i < columns; ++i) {
			current += columnWidth;
			int nextX = rect.X + (int)(current * workingWidth);
			result[i] = new Rectangle( lastX + i * colMargin, rect.Y, nextX - lastX, rect.Height );
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

	#region InflateBy, Fit

	/// <returns>a new rectangle inflated by # of pixels on each side</returns>
	static public Rectangle InflateBy( this Rectangle rect, int delta )
		=> new Rectangle( rect.X - delta, rect.Y - delta, rect.Width + delta * 2, rect.Height + delta * 2 );

	static public RectangleF InflateBy( this RectangleF rect, float delta )
		=> new RectangleF( rect.X - delta, rect.Y - delta, rect.Width + delta * 2, rect.Height + delta * 2 );

	static public Rectangle InflateBy( this Rectangle rect, int deltaX, int deltaY )
		=> new Rectangle( rect.X - deltaX, rect.Y - deltaY, rect.Width + deltaX * 2, rect.Height + deltaY * 2 );

	static public RectangleF InflateBy( this RectangleF rect, float deltaX, float deltaY )
		=> new RectangleF( rect.X - deltaX, rect.Y - deltaY, rect.Width + deltaX * 2, rect.Height + deltaY * 2 );

	static public Rectangle OffsetBy( this Rectangle rect, int deltaX, int deltaY ) 
		=> new Rectangle( rect.X + deltaX, rect.Y + deltaY, rect.Width, rect.Height );


	static public Rectangle FitBoth( this Rectangle bounds, float widthRatio, Align horizontal = default, Align vertical = default ) 
		=> bounds.Height * widthRatio < bounds.Width
			? bounds.FitHeight( new Size((int)(bounds.Height*widthRatio),bounds.Height), horizontal )
			: bounds.FitWidth( new Size(bounds.Width,(int)(bounds.Width/widthRatio)), vertical );
	static public Rectangle FitBoth( this Rectangle bounds, int width, int height, Align horizontal = default, Align vertical = default ) 
		=> bounds.FitBoth(new Size(width,height),horizontal,vertical);
	static public Rectangle FitBoth( this Rectangle bounds, Size size, Align horizontal = default, Align vertical = default )
		=> bounds.Height * size.Width < size.Height * bounds.Width
			? bounds.FitHeight( size, horizontal )
			: bounds.FitWidth( size, vertical );

	static public Rectangle FitHeight( this Rectangle bounds, Size size, Align align = default ) {
		// Centered Horizontally
		int width = bounds.Height * size.Width / size.Height;
		return align switch {
			Align.Near => new Rectangle( bounds.X, bounds.Y, width, bounds.Height ),
			Align.Far => new Rectangle( bounds.Right - width, bounds.Y, width, bounds.Height ),
			_ => new Rectangle( bounds.X + (bounds.Width - width) / 2, bounds.Y, width, bounds.Height ), // center / default
		};
	}

	public static Rectangle FitWidth( this Rectangle bounds, Size size, Align align = default ) {
		int height = bounds.Width * size.Height / size.Width;
		return align switch {
			Align.Near => new Rectangle( bounds.X, bounds.Y, bounds.Width, height ),
			Align.Far => new Rectangle( bounds.X, bounds.Bottom - height, bounds.Width, height ),
			_ => new Rectangle( bounds.X, bounds.Y + (bounds.Height - height) / 2, bounds.Width, height ), // center / default
		};
	}

	/// <summary>
	/// Applies the % padding (0..1) to the minimum(width,height)
	/// </summary>
	/// <returns>A new Rectangle</returns>
	public static Rectangle Pad( this Rectangle bounds, float paddingPercent ){
		if(paddingPercent == 0f) return bounds;
		int padding = (int)(Math.Min(bounds.Height,bounds.Width) * paddingPercent + .5f);
		return bounds.InflateBy( -padding );
	}

	public static Rectangle PadTopBottom( this Rectangle bounds, float paddingPercent ){
		if(paddingPercent == 0f) return bounds;
		int padding = (int)(Math.Min(bounds.Height,bounds.Width) * paddingPercent + .5f);
		return bounds.InflateBy( 0, -padding );
	}


	public static Point TL( this Rectangle rect ) => rect.Location;
	public static Point BL( this Rectangle rect ) => new Point(rect.Left,rect.Bottom);
	public static Point BR( this Rectangle rect ) => new Point(rect.Right,rect.Bottom);
	public static Point TR( this Rectangle rect ) => new Point(rect.Top,rect.Bottom);


	#endregion

	static public Size FitWidth( this Size sz, int width ) => new Size( width, width * sz.Height / sz.Width );

}

