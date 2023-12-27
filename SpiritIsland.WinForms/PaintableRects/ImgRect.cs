using System.Drawing;

namespace SpiritIsland.WinForms;

public class ImgRect : IPaintableRect {

	static ImgSource _imageSource = ResourceImages.Singleton; // could be replaced with something that caches the images.

	public ImgRect(Img img ) { _img=img; }
	readonly Img _img;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		using Bitmap image = _imageSource.GetImg( _img );
		float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
		var fitted = rect.FitBoth( image.Size );
		graphics.DrawImage(image, fitted );
		return fitted;
	}

}
