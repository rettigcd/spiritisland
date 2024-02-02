namespace SpiritIsland;

public class HorizontalStackRect : IPaintableRect {
	readonly IPaintableRect[] _children;
	Func<Rectangle, Rectangle[]> _splitter;

	public float? WidthRatio { get; set; }

	public HorizontalStackRect( params IPaintableRect[] children ) {
		_children = children;
		_splitter = SplitEqually;
	}
	public HorizontalStackRect SplitByWeight( float margin, params float[] weights ) {
		_splitter = ( bounds ) => bounds.SplitHorizontallyByWeight( margin, weights );
		return this;
	}
	Rectangle[] SplitEqually( Rectangle rect ) => rect.SplitHorizontallyIntoColumns( 0, _children.Length );
	public void Paint( Graphics graphics, Rectangle rect ) {
		var rects = _splitter( rect );
		for(int i = 0; i < _children.Length; ++i)
			_children[i].Paint( graphics, rects[i] );
	}
}
