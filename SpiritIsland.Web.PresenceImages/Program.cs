using System.Drawing;
using System.Drawing.Imaging;
using SpiritIsland;
using SpiritIsland.Web.Oracle;

// Writes SpiritIsland.Web/client/public/img/presence/{spirit name, spaces as underscores}.png for every spirit the web game offers:
// the presence shape with the spirit's picture laid over it, tuned per spirit in SpiritMarkerBuilder (what the desktop apps draw).

string folder = args.Length > 0 ? args[0] : FindFolder();
Directory.CreateDirectory( folder );

// The desktop code loads the spirits' pictures from .\images/spirits, relative to where the program runs.
Directory.SetCurrentDirectory( AppContext.BaseDirectory );

List<string> drawn = [];
foreach( string name in GameSetup.Spirits ) {
	GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( new NewGameRequest( name, "A", Seed: 1 ) ) );
	Spirit spirit = gs.Spirits.Single();
	string file = Path.Combine( folder, name.Replace( ' ', '_' ) + ".png" );
	try {
		using Bitmap marker = SpiritMarkerBuilder.BuildSpiritMarker( spirit, Img.Token_Presence, ResourceImages.Singleton );
		marker.Save( file, ImageFormat.Png );
		Console.WriteLine( $"{name} -> {Path.GetFileName( file )}" );
		drawn.Add( name );
	}
	catch( FileNotFoundException ) {
		// no picture of this spirit, so nothing to lay over the shape: the web client draws its plain presence for it
		Console.WriteLine( $"{name}: no picture in images/spirits, skipped" );
	}
}

// Which spirits have a picture, for the client to know when to ask for one (the rest get its plain presence).
string list = Path.GetFullPath( Path.Combine( folder, "..", "..", "..", "src", "presenceImages.json" ) );
File.WriteAllText( list, System.Text.Json.JsonSerializer.Serialize( drawn, new System.Text.Json.JsonSerializerOptions { WriteIndented = true } ) + "\n" );
Console.WriteLine( $"{drawn.Count} presence pictures written to {folder}, and their names to {list}" );

static string FindFolder() {
	for( DirectoryInfo? dir = new( Directory.GetCurrentDirectory() ); dir is not null; dir = dir.Parent ) {
		string candidate = Path.Combine( dir.FullName, "SpiritIsland.Web", "client", "public", "img", "presence" );
		if( Directory.Exists( candidate ) ) return candidate;
	}
	throw new DirectoryNotFoundException( "Run this from inside the repository, or pass the folder to write to." );
}
