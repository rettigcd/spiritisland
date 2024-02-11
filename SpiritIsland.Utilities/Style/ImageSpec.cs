namespace SpiritIsland;

public abstract class ImageSpec( bool shouldDispose ) {

	#region static Implicit converters

	// borrowd
	static public implicit operator ImageSpec( Bitmap bitmap ) => new BitmapImage( bitmap );
	// owned
	static public implicit operator ImageSpec( Img img ) => new ImgImage( ImageCache, img );
	static public implicit operator ImageSpec(IconDescriptor icon) => new IconImage( ImageCache, icon );
	static public implicit operator ImageSpec(PowerCard card) => new PowerCardImage( ImageCache, card );
	static public ImageSpec From( IToken token ) => new TokenImgSource( ImageCache, token );
	static public implicit operator ImageSpec( Func<Bitmap> img ) => new FuncImage( img ); // !!! this seems dangerous since we don't know disposal histroy
	// mystery
	// static public implicit operator ImageSpec(Func<ResourceMgr<Bitmap>> func) => new ResourceMgrImage(func);

	#endregion static Implicit converters

	public virtual ResourceMgr<Bitmap> GetResourceMgr() => new ResourceMgr<Bitmap>( GetBitmap(), _owned );
	public abstract Bitmap GetBitmap();

	protected bool _owned = shouldDispose;

	// Borrowed - don't dispose, don't cache
	class BitmapImage( Bitmap bitmap ) : ImageSpec( false ) { public override Bitmap GetBitmap() => bitmap; }
	// Borrowed - (Cached from ResouceImages) - Dispose, may cache
	class ImgImage( ResourceImageSource resImages, Img img ) : ImageSpec(resImages.CalleeShouldDisposeOfResource){ public override Bitmap GetBitmap() => resImages.GetImg(img ); }
	class IconImage( ResourceImageSource resImages, IconDescriptor icon) : ImageSpec( resImages.CalleeShouldDisposeOfResource ) { public override Bitmap GetBitmap() => resImages.GetTrackSlot( icon ); }
	class TokenImgSource( ResourceImageSource resImages, IToken token ) : ImageSpec( resImages.CalleeShouldDisposeOfResource ) { public override Bitmap GetBitmap() => resImages.GetTokenImage( token ); }
	class PowerCardImage( ResourceImageSource resImages, PowerCard card ) : ImageSpec( resImages.CalleeShouldDisposeOfResource ) { public override Bitmap GetBitmap() => resImages.GetPowerCard( card ); }

	// Ownded (generic) - Dispose, may cache
	class FuncImage( Func<Bitmap> generator ) : ImageSpec( true ) { public override Bitmap GetBitmap() => generator(); }

	//  Owned - What the heck is this?
	class ResourceMgrImage( Func<ResourceMgr<Bitmap>> func ) : ImageSpec( true ) {
		public override ResourceMgr<Bitmap> GetResourceMgr() => func();
		public override Bitmap GetBitmap() => _dummy; static readonly Bitmap _dummy = new Bitmap(1,1);
	}

	#region static Cache

	static public readonly CachedImageSource ImageCache = new CachedImageSource( ResourceImages.Singleton );

	#endregion static Cache

}

public class CachedImageSource( ResourceImageSource _nonCached ) : ResourceImageSource {
	bool ResourceImageSource.CalleeShouldDisposeOfResource => false;

	#region public GetImg versions

	public Bitmap GetImg( Img img ) {
		if(GetSaved( img, out Bitmap? bitmap )) return bitmap!;
		DateTime start = DateTime.Now;
		bitmap = _nonCached.GetImg( img );
		TimeSpan loadTime = DateTime.Now - start;
		Save( img, bitmap, loadTime );
		return bitmap;
	}

	public Bitmap GetTokenImage( IToken token ) {
		if(GetSaved( token, out Bitmap? bitmap )) return bitmap!;
		DateTime start = DateTime.Now;
		bitmap = _nonCached.GetTokenImage( token );
		TimeSpan loadTime = DateTime.Now - start;
		Save( token, bitmap, loadTime );
		return bitmap;
	}
	public Bitmap GetTrackSlot( IconDescriptor icon ) {
		if(GetSaved( icon, out Bitmap? bitmap )) return bitmap!;
		DateTime start = DateTime.Now;
		bitmap = _nonCached.GetTrackSlot( icon );
		TimeSpan loadTime = DateTime.Now - start;
		Save( icon, bitmap, loadTime );
		return bitmap;
	}

	public Bitmap GetPowerCard( PowerCard card ) {
		if(GetSaved( card, out Bitmap? bitmap )) return bitmap!;
		DateTime start = DateTime.Now;
		bitmap = _nonCached.GetPowerCard( card );
		TimeSpan loadTime = DateTime.Now - start;
		Save( card, bitmap, loadTime );
		return bitmap;
	}

	#endregion public GetImg versions

	#region private Save/Load

	bool GetSaved( object key, out Bitmap? bitmap ) {
		if(_cache.TryGetValue( key, out CacheInfo? info )) {
			info.RequestCount++;
			bitmap = info.Bitmap;
			return true;
		}
		bitmap = null;
		return false;
	}

	void Save( object key, Bitmap bitmap, TimeSpan loadTime ) {
		var info = new CacheInfo( bitmap, loadTime );
		_cache.Add( key, info );
		TotalLoadTime += loadTime;
		MemoryUsage += info.MemoryUsage;
	}

	#endregion private Save/Load

	/// <summary> Not threadsafe, call only from UI thread that doesthe painting. </summary>
	public void Clear() {
		foreach(var info in _cache.Values.ToArray())
			info.Bitmap.Dispose();
		_cache.Clear();
		MemoryUsage = 0;
	}
	/// <summary> Running total. Doesn't reset </summary>
	public TimeSpan TotalLoadTime;
	/// <summary> Current memory usage. Resets with call to Clear() </summary>
	public int MemoryUsage;

	readonly Dictionary<object, CacheInfo> _cache = [];

	public class CacheInfo( Bitmap bitmap, TimeSpan loadTime ) {
		public readonly Bitmap Bitmap = bitmap;
		public readonly int MemoryUsage = bitmap.Width * bitmap.Height * 4;
		public readonly TimeSpan LoadTime = loadTime;
		public TimeSpan Saved => LoadTime.Multiply( RequestCount - 1 );
		public int RequestCount = 1;
		public override string ToString() => $"{Saved} - {RequestCount} / {MemoryUsage}";
	}
}
