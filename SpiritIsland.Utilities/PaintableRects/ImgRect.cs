using System.Drawing;

namespace SpiritIsland;

public class ImgRect : IPaintableRect {

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
	readonly static ImgSource _imageSource = ResourceImages.Singleton; // could be replaced with something that caches the images.
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

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
