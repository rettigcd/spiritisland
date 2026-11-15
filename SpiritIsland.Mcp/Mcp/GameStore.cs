using System.Text.Json.Nodes;
using ModelContextProtocol;
using SpiritIsland.Agent;

namespace SpiritIsland.Mcp;

/// <summary>
/// Keeps each game on disk (one JSON file per game, see <see cref="AgentGame.Save"/>) instead of in memory, so any number of
/// games can be in progress and nothing - no engine task, no thread - is held between tool calls. A call restores the game,
/// works on it, writes it back and lets the instance go. Calls are serialized: the engine's ambient scope is per execution
/// context, and a restored game is only valid for the call that restored it.
/// Directory: env var SPIRITISLAND_GAMES_DIR, else a games folder next to the executable.
/// </summary>
public static class GameStore {

	public static string Directory { get; } = Environment.GetEnvironmentVariable( "SPIRITISLAND_GAMES_DIR" )
		is { Length: > 0 } p ? p : Path.Combine( AppContext.BaseDirectory, "games" );

	/// <summary> Starts a game, saves it, and returns its id along with whatever <paramref name="work"/> returns. </summary>
	public static string Create( NewGameRequest request, Func<AgentGame, string, string> work ) {
		lock( _lock ) {
			AgentGame game;
			try { game = new AgentGame( request ); }
			catch( ArgumentException ex ) { throw new McpException( ex.Message ); }
			string id = Guid.NewGuid().ToString( "N" )[..8];
			string result = work( game, id );
			Write( id, game );
			return result;
		}
	}

	/// <summary>
	/// Restores the game, runs <paramref name="work"/> on it and saves it again. A game id of null means the game saved most
	/// recently. If <paramref name="work"/> throws, the saved game is left as it was.
	/// </summary>
	public static string Use( string? gameId, Func<AgentGame, string> work ) {
		lock( _lock ) {
			string id = gameId is null ? Latest() : Validate( gameId );
			string path = PathOf( id );
			if( !File.Exists( path ) )
				throw new McpException( $"No game '{id}'. Games: {string.Join( ", ", Ids() )}. Call create_game to start one." );
			AgentGame game = AgentGame.Restore( JsonNode.Parse( File.ReadAllText( path ) )!.AsObject() );
			string result = work( game );
			Write( id, game );
			return result;
		}
	}

	/// <summary> One line per saved game: its id, setup, round and whether it is over. </summary>
	public static JsonArray List() {
		lock( _lock ) {
			var games = new JsonArray();
			foreach( string id in Ids() ) {
				JsonObject save = JsonNode.Parse( File.ReadAllText( PathOf( id ) ) )!.AsObject();
				var request = save["Request"]!;
				games.Add( new JsonObject {
					["gameId"] = id,
					["spirit"] = request["Spirit"]!.DeepClone(),
					["board"] = request["Board"]!.DeepClone(),
					["seed"] = request["Seed"]!.DeepClone(),
					["adversary"] = request["Adversary"]?.DeepClone(),
					["decisionsMade"] = save["History"]!.AsArray().Count,
					["lastPlayed"] = File.GetLastWriteTime( PathOf( id ) ).ToString( "s" ),
				} );
			}
			return games;
		}
	}

	static void Write( string id, AgentGame game ) {
		System.IO.Directory.CreateDirectory( Directory );
		string path = PathOf( id );
		string temp = path + ".tmp"; // a crash mid-write leaves the previous save intact
		File.WriteAllText( temp, game.Save().ToJsonString() );
		File.Move( temp, path, overwrite: true );
	}

	static string PathOf( string id ) => Path.Combine( Directory, id + ".json" );

	static string Validate( string id )
		=> id.All( char.IsAsciiLetterOrDigit ) ? id : throw new McpException( $"'{id}' is not a game id." );

	static string[] Ids() => !System.IO.Directory.Exists( Directory ) ? []
		: [.. new DirectoryInfo( Directory ).GetFiles( "*.json" ).OrderByDescending( f => f.LastWriteTimeUtc )
			.Select( f => Path.GetFileNameWithoutExtension( f.Name ) )];

	static string Latest() => Ids() is [var newest, ..] ? newest
		: throw new McpException( "No game in progress. Call create_game first." );

	static readonly object _lock = new();
}
