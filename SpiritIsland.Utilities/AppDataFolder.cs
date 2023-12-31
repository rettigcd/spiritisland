namespace SpiritIsland;

/// <summary>
/// Simplifies accessing a Spirit-Island App-Data folder
/// </summary>
public static class AppDataFolder {
	public static string GetRootPath() {
		string folder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), "SpiritIsland" );
		if(!Directory.Exists( folder ))
			Directory.CreateDirectory( folder );
		return folder;
	}
	public static string GetSubFolderPath( string name ) {
		string folder = Path.Combine( GetRootPath(), name );
		if(!Directory.Exists( folder ))
			Directory.CreateDirectory( folder );
		return folder;
	}

}