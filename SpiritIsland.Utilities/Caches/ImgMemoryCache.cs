using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Pulls images from ResoueceImages/Files, and Caches them
/// </summary>
/// <remarks>
/// Might speed up drawing if used by things that call ResourceImages.Singleton.GetImage()
/// </remarks>
public sealed class ImgMemoryCache : IDisposable {

	public ImgMemoryCache() {}

	public Image GetImage( Img img ) {   // !!! make this private
		if(_images.TryGetValue( img, out Image? bob )) return bob!;
		var image = ResourceImages.Singleton.GetImg( img );
		_images.Add( img, image );
		return image;
	}

	public void Dispose() {
		foreach(var img in _images.Values)
			img.Dispose();
		_images.Clear();
	}

	#region private fields
	readonly Dictionary<Img, Image> _images = new Dictionary<Img, Image>();
	#endregion
}
