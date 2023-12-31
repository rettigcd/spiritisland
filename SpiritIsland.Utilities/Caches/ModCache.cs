using System.Drawing;

namespace SpiritIsland;

public class ModCache {

	static public ModCache Ghosts( ImgSource imgSource ) => new ModCache( imgSource, "ghosts", SimpleMods.Ghostify );

	static public ModCache Grays( ImgSource imgSource ) => new ModCache( imgSource, "grays", SimpleMods.Grayify );

	ModCache( ImgSource imgSource, string dir, Action<Bitmap> mod ) {
		_imgSource = imgSource;
		_directory = dir;
		_mod = mod;
	}

	public Bitmap GetImage( Img img ) {

		string key = $"ghosts\\{img}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = _imgSource.GetImg( img );
		_mod( image );
		_cache.Add( key, image );
		return image;
	}


	readonly string _directory;
	readonly Action<Bitmap> _mod;
	readonly ImgSource _imgSource;
	readonly ImageDiskCache _cache = new ImageDiskCache();

}
