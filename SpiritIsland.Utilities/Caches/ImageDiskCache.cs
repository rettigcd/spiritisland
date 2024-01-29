using System.Drawing.Imaging;

namespace SpiritIsland;

/// <summary>
/// Caches Images on Disk
/// </summary>
public class ImageDiskCache {

	#region constructor

	public ImageDiskCache() {
		_folder = AppDataFolder.GetSubFolderPath( "cache" );
	}

	#endregion

	public bool Contains( string key ) => File.Exists( GetPath( key ) );

	public Bitmap Get(string key) {
		string path = GetPath( key );
		return File.Exists( path ) 
			? (Bitmap)Image.FromFile( path ) // all file formats supported by this returns a bitmap.
			: throw new InvalidOperationException( $"Path does not exist. {path}" );
	}

	public void Add( string key, Bitmap bitmap ) {
		ImageFormat imageFormat = key.Contains( "png" ) ? ImageFormat.Png : ImageFormat.Jpeg;
		SaveBmp( bitmap, GetPath( key ), imageFormat );
	}

	#region private

	static public void SaveBmp( Bitmap bmp, string filePathAndName, ImageFormat imageFormat ) {

		// Specify image quality.
		const long quality = 80L; // 1..100?
		var encoderParameters = new EncoderParameters( 1 );
		encoderParameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );

		// Enshure subfolder exists.
		string directory = System.IO.Path.GetDirectoryName( filePathAndName ) ?? string.Empty;
		if(!Directory.Exists( directory ))
			Directory.CreateDirectory( directory );

		bmp.Save( filePathAndName, GetCodec( imageFormat ), encoderParameters );
	}

	/// <summary> Finds the ImageCodec the supports the given ImageFormat </summary>
	static ImageCodecInfo GetCodec( ImageFormat imageFormat ) {
		ImageCodecInfo? codec = ImageCodecInfo.GetImageDecoders()
			.FirstOrDefault( codec => codec.FormatID == imageFormat.Guid );
		return codec is not null ? codec 
			: throw new ArgumentException( $"Unable to save image. Codec {imageFormat} not found." );
	}

	string GetPath( string key ) => Path.Combine( _folder, key );
	readonly string _folder;

	#endregion

}
