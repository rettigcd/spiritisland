namespace SpiritIsland;

public class HorizontalStackRect : IPaintableRect {

	public float? WidthRatio { get; set; }

	#region constructor
	public HorizontalStackRect( params IPaintableRect[] children ) {
		_children = children;
		_splitter = SplitEqually;
	}

	#endregion
	public HorizontalStackRect SplitByWeight( float margin, params float[] weights ) {
		_splitter = ( bounds ) => bounds.SplitHorizontallyByWeight( margin, weights );
		return this;
	}
	public void Paint( Graphics graphics, Rectangle rect ) {
		var rects = _splitter( rect );
		for(int i = 0; i < _children.Length; ++i)
			_children[i].Paint( graphics, rects[i] );
	}

	#region private
	Rectangle[] SplitEqually( Rectangle rect ) => rect.SplitHorizontallyIntoColumns( 0, _children.Length );

	Func<Rectangle, Rectangle[]> _splitter;
	readonly IPaintableRect[] _children;

	#endregion

}
