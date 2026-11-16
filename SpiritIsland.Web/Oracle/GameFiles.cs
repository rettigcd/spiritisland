using System.Text.Json.Nodes;

namespace SpiritIsland.Web.Oracle;

/// <summary> One line per saved game, for a game list. </summary>
public record GameSummary( string GameId, string Spirit, string Board, int Seed, string? Adversary, bool Over, DateTime LastPlayed );

/// <summary> Where games are kept. Multiplayer will need a different format, so nothing else depends on files. </summary>
public interface IGameFiles {
	string NewId();
	GameRecord? Read( string id );
	void Write( GameRecord record );
	IReadOnlyList<GameSummary> List();
}

/// <summary> One JSON file per game in a directory. </summary>
public sealed class GameFiles( string directory ) : IGameFiles {

	public string NewId() => Guid.NewGuid().ToString( "N" )[..8];

	public GameRecord? Read( string id ) {
		if( !IsValidId( id ) ) return null;
		string path = PathOf( id );
		return File.Exists( path ) ? GameRecord.FromJson( JsonNode.Parse( File.ReadAllText( path ) )!.AsObject() ) : null;
	}

	public void Write( GameRecord record ) {
		Directory.CreateDirectory( directory );
		string path = PathOf( record.Id );
		string temp = path + ".tmp"; // a crash mid-write leaves the previous save intact
		File.WriteAllText( temp, record.ToJson().ToJsonString() );
		// On Windows, a virus scanner or indexer can briefly hold the file we just wrote, so the replace is denied. Retry.
		for( int attempt = 1; ; ++attempt ) {
			try {
				File.Move( temp, path, overwrite: true );
				return;
			} catch( Exception ex ) when( ex is UnauthorizedAccessException or IOException && attempt < 5 ) {
				Thread.Sleep( 20 * attempt );
			}
		}
	}

	public IReadOnlyList<GameSummary> List() {
		if( !Directory.Exists( directory ) ) return [];
		return [.. new DirectoryInfo( directory ).GetFiles( "*.json" )
			.OrderByDescending( f => f.LastWriteTimeUtc )
			.Select( f => (File: f, Record: Read( Path.GetFileNameWithoutExtension( f.Name ) )) )
			.Where( x => x.Record is not null )
			.Select( x => new GameSummary(
				x.Record!.Id, x.Record.Config.Spirits![0], x.Record.Config.Boards![0], x.Record.Config.ShuffleNumber,
				x.Record.Config.Adversary?.Name, x.Record.Over, x.File.LastWriteTime ) )];
	}

	// ids become file names
	static bool IsValidId( string id ) => id.Length is > 0 and <= 32 && id.All( char.IsAsciiLetterOrDigit );

	string PathOf( string id ) => Path.Combine( directory, id + ".json" );
}
