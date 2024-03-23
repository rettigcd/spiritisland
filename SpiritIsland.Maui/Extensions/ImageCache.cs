namespace SpiritIsland.Maui;

static public class ImageCache {

	static public ImageSource FromFile( string filename) {
		if(_dict.TryGetValue(filename, out ImageSource? imageSource) ) 
			return imageSource;
		var imgSrc = ImageSource.FromFile(filename);
		_dict.Add(filename, imgSrc );
		return imgSrc;
	}

	static readonly Dictionary<string,ImageSource> _dict = [];

}