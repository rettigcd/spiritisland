using System.Drawing;

namespace SpiritIsland.WinForms;

class ImgRect : IPaintableRect {
	public ImgRect(Img img ) { _img=img; }
	readonly Img _img;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		using Bitmap image = ResourceImages.Singleton.GetImage( _img );
		float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
		var fitted = rect.FitBoth( image.Size );
		graphics.DrawImage(image, fitted );
		return fitted;
	}

}
