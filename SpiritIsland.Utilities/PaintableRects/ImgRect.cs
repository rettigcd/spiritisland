using System.Drawing;

namespace SpiritIsland;

public class ImgRect( Img img ) : IPaintableRect {

	static public Func<Img,ResourceMgr<Bitmap>> GetBitmap = DefaultGetBitmap;

	static ResourceMgr<Bitmap> DefaultGetBitmap(Img img) => new ResourceMgr<Bitmap>( ResourceImages.Singleton.GetImg(img), true );

	readonly public static ImgSource _imageSource = ResourceImages.Singleton; // could be replaced with something that caches the images.

	readonly Img _img = img;

	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		using ResourceMgr<Bitmap> mgr = GetBitmap(_img);
		Bitmap image = mgr.Resource;
		float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
		var fitted = rect.FitBoth( image.Size );
		graphics.DrawImage(image, fitted );
		return fitted;
	}

}
