using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Pulls images from ResoueceImages/Files, and Caches them
/// </summary>
public sealed class CachedImageDrawer : IDisposable {

	readonly Dictionary<Img,Image> _images = new Dictionary<Img,Image>();

	public CachedImageDrawer() {}

	public void Draw(Graphics graphics, Img img, RectangleF rect) {
		graphics.DrawImage( GetImage(img), rect );
	}

	public void DrawFitHeight( Graphics graphics, Img img, Rectangle rect ) {
		graphics.DrawImageFitHeight(  GetImage( img ), rect );
	}

	public Image? GetImage( Img img ) {   // !!! make this private
		if(_images.TryGetValue( img, out Image? bob )) return bob;
		var image = ResourceImages.Singleton.GetImage( img );
		_images.Add( img, image );
		return image;
	}

	public void Dispose() {
		foreach(var img in _images.Values)
			img.Dispose();
		_images.Clear();
	}
}
