namespace SpiritIsland;

public abstract class PaddingSpec {
	
	static public readonly PaddingSpec None = new NullPad();

	static public implicit operator PaddingSpec(float percent) => new PercentageOfMinDimension( percent );
	static public implicit operator PaddingSpec((float w,float h) pair) => new PercentageOfMinDimension( pair.w, pair.h );

	public abstract Rectangle Content(Rectangle bounds);
	public abstract Size Pad(Size childSize);

	class NullPad : PaddingSpec{ 
		public override Rectangle Content( Rectangle bounds ) => bounds;
		public override Size Pad( Size childSize ) => childSize;
	}

	class PercentageOfMinDimension : PaddingSpec {
		public PercentageOfMinDimension( float percent ){ 
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(percent,.5f,nameof(percent));
			_widthPercent = _heightPercent = percent;
		}
		public PercentageOfMinDimension( float w, float h ){ 
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(w,.5f,nameof(w));
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(h,.5f,nameof(h));
			_widthPercent = w; 
			_heightPercent = h;
		}
		public override Rectangle Content( Rectangle bounds ){
			if(_widthPercent == 0f && _heightPercent == 0 ) return bounds;
			int min = Min(bounds.Size);
			return bounds.InflateBy( 
				-(int)(min*_widthPercent+.5f),
				-(int)(min*_heightPercent+.5f)
			);
		}
		public override Size Pad( Size contentSize ){
			if( contentSize.Width < contentSize.Height ){
				// use Width as min
				int paddedWidth = (int)(contentSize.Width / (1-2*_widthPercent));
				return new Size( paddedWidth, contentSize.Height + (int)(2*paddedWidth*_heightPercent) );
			} else {
				// use Height as min
				int paddedHeight = (int)(contentSize.Height / (1-2*_heightPercent));
				return new Size( contentSize.Width + (int)(2*paddedHeight*_widthPercent), paddedHeight );
			}
		}
		static int Min(Size size) => Math.Min(size.Width,size.Height);
		readonly float _widthPercent;
		readonly float _heightPercent;
	}
}