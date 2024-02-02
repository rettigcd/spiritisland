namespace SpiritIsland;

public class ImgRect( ImageSpec img ) : IPaintableRect {

	public ImageSpec Img {
		get => _img;
		set{
			_img = value;
			if(_mgr is not null){ _mgr.Dispose(); _mgr = null; }
			_widthRatio = null;
		}
	}
		
	public float? WidthRatio => _widthRatio ??= Mgr.Resource.Width * 1f / Mgr.Resource.Height;

	public void Paint( Graphics graphics, Rectangle bounds ) {
		using( ResourceMgr<Bitmap> mgr = Mgr ){
			Bitmap image = mgr.Resource;
			bounds = bounds.FitBoth( image.Size );
			graphics.DrawImage(image, bounds );
		}
		_mgr = null;
	}

	ImageSpec _img = img;
	ResourceMgr<Bitmap> Mgr => _mgr ??= _img.GetResourceMgr();
	ResourceMgr<Bitmap>? _mgr;
	float? _widthRatio;

}
