namespace SpiritIsland;

public class ImgRect( ImageSpec img ) : IPaintableRect {

	public PenSpec? Border {get;set;}
	public BrushSpec? Background {get;set;}

	/// <summary> returns the last Bounds less the Margin. </summary>
	public Rectangle Bounds {get; private set;}

	/// <summary> (% of Min(width,height)) to add around the border</summary>
	public PaddingSpec Padding = PaddingSpec.None;
	public PaddingSpec Margin = PaddingSpec.None;

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
		Bounds = Margin.Content(bounds);
		Background?.Fill(graphics,Bounds);
		PaintContent(graphics,Padding.Content(Bounds));
		Border?.DrawRectangle(graphics,Bounds); // doing last to clean-up edges
	}

	void PaintContent( Graphics graphics, Rectangle content ) {
		using( ResourceMgr<Bitmap> mgr = Mgr ){
			Bitmap image = mgr.Resource;
			content = content.FitBoth( image.Size );
			graphics.DrawImage(image, content );
		}
		_mgr = null;
	}

	ImageSpec _img = img;
	ResourceMgr<Bitmap> Mgr => _mgr ??= _img.GetResourceMgr();
	ResourceMgr<Bitmap>? _mgr;
	float? _widthRatio;

}
