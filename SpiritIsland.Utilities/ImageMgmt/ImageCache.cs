using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SpiritIsland.WinForms;

// Save because:
//	Resize
//	color / hue / etc Adjustment
//	constructed

class ImageCache {

	#region constructor

	public ImageCache() {
		_folder = DataFolder.GetSubFolderPath( "cache" );
	}

	#endregion

	public bool Contains( string key ) => File.Exists( GetPath( key ) );

	public Image Get(string key) {
		string path = GetPath( key );
		return File.Exists( path ) ? Image.FromFile( path ) : null;
	}

	public void Add( string key, Bitmap bitmap ) {
		ImageFormat imageFormat = key.Contains( "png" ) ? ImageFormat.Png : ImageFormat.Jpeg;
		SaveBmp( bitmap, GetPath( key ), imageFormat );
	}

	#region private

	static void SaveBmp( Bitmap bmp, string filePathAndName, ImageFormat imageFormat ) {
		var codec = ImageCodecInfo.GetImageDecoders()
			.FirstOrDefault( codec => codec.FormatID == imageFormat.Guid );
		const long quality = 80L; // 1..100?
		var encoderParameters = new EncoderParameters( 1 );
		encoderParameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );

		// Enshure subfolder exists.
		string? directory = System.IO.Path.GetDirectoryName( filePathAndName );
		if(!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		bmp.Save( filePathAndName, codec, encoderParameters );
	}

	string GetPath( string key ) => Path.Combine( _folder, key );
	readonly string _folder;

	#endregion

}
