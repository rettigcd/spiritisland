namespace SpiritIsland;

public interface IPaintableRect {
	void Paint( Graphics graphics, Rectangle bounds );

	/// <summary>
	/// Set if the Paintable has instrinsic dimentions.
	/// </summary>
	float? WidthRatio { get; }
}


static public class IPaintableRect_Extensions {

	static public IPaintableRect RiseAbove( this IPaintableRect above, List<IPaintAbove> aboves ){
		var wrapper = new UpperLayer(above);
		aboves.Add(wrapper);
		return wrapper;
	}

	static public IPaintableRect ShowIf( this IPaintableRect inner, Func<bool> condition ){
		return new ConditionalRect( inner, condition );
	}

	/// <summary>
	/// Handy for stripping the WidthRatio off of an object so RowRect can evently distribute it.
	/// </summary>
	static public PoolRect FloatSelf( this IPaintableRect inner) => new PoolRect().Float(inner);
	static public PoolRect FloatSelf_WithRatio( this IPaintableRect inner) => new PoolRect(){WidthRatio = inner.WidthRatio}.Float(inner);

	// specifies floating position with integer % (0..100)
	static public PoolRect FloatSelf( this IPaintableRect inner, int x, int y, int w, int h) 
		=> new PoolRect().Float(inner,x,y,w,h);	

	static public IPaintableRect Cache( this IPaintableRect child, HashSet<IDisposable> all ) => new CachedBitmapRect(child,all);

}

public class ConditionalRect( IPaintableRect inner, Func<bool> shouldPaint ) : IPaintableRect {

 	readonly IPaintableRect _inner = inner;
	readonly Func<bool> _shouldPaint = shouldPaint;

	public float? WidthRatio => _inner.WidthRatio;

	public void Paint(Graphics graphics, Rectangle bounds) {
		if(_shouldPaint())
			_inner.Paint(graphics,bounds);
	}

}