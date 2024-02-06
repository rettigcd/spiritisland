using System.Drawing.Drawing2D;

namespace SpiritIsland;

/// <summary>
/// Caches the children
/// </summary>
public sealed class CachedBitmapRect( IPaintableRect child, HashSet<IDisposable> all ) : IPaintableRect, IDisposable {
	public float? WidthRatio => _child.WidthRatio;

	public void Paint( Graphics graphics, Rectangle bounds ){
		if(bounds != _bounds){ Dispose(); _bounds=bounds; }
		_backgroundCache ??= CacheBackground(bounds);
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

	readonly IPaintableRect _child = child;
	readonly HashSet<IDisposable> _all = all;

}