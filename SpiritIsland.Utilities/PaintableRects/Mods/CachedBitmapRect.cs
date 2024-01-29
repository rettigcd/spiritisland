using System.Drawing.Drawing2D;

namespace SpiritIsland;

/// <summary>
/// Caches the children
/// </summary>
public sealed class CachedBitmapRect : IPaintableRect, IDisposable {
	public CachedBitmapRect( IPaintableRect child, HashSet<IDisposable> all ){
		_child = child;
		_all = all;
	}

	public float? WidthRatio => _child.WidthRatio;

	public void Paint( Graphics graphics, Rectangle bounds ){
		if(bounds != _bounds){ Dispose(); _bounds=bounds; }
		if(_backgroundCache is null)
			_backgroundCache = CacheBackground(bounds);
		graphics.DrawImage(_backgroundCache,_bounds);
	}

	Bitmap CacheBackground(Rectangle bounds){
		var bgCache = new Bitmap( bounds.Width, bounds.Height );
		using var graphics = Graphics.FromImage( bgCache );
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; 
		graphics.TranslateTransform(-bounds.X,-bounds.Y);
		_child.Paint(graphics,bounds);
		_all.Add(this);
		return bgCache;
	}

	public void Dispose(){
		_backgroundCache?.Dispose();
		_backgroundCache = null;
		_all.Remove(this);
	}

	Bitmap? _backgroundCache;
	Rectangle _bounds;

	readonly IPaintableRect _child;
	readonly HashSet<IDisposable> _all;

}