using System.IO;

namespace SpiritIsland.SinglePlayer;

/// <summary>
/// Reads and writes a suspended SoloGame as a single JSON file: the GameConfiguration that rebuilds the
/// starting game, plus <see cref="SoloGame.Save"/>'s state on top of it. Nothing in the engine requires this -
/// a game can stay in memory for its whole life - it is only for suspending a game and shutting down.
/// Rewind history is not part of the file; a resumed game starts with nothing to rewind to.
/// </summary>
public static class SoloGameFile {

	const int FormatVersion = 1;

	public static JsonObject ToJson( GameConfiguration config, SoloGame game ) => ToJson( config, game.Save() );

	/// <param name="gameSave">A <see cref="SoloGame.Save"/>, or <see cref="SoloGame.LatestCheckpoint"/>.</param>
	public static JsonObject ToJson( GameConfiguration config, JsonObject gameSave ) => new() {
		["Version"] = FormatVersion,
		["Config"] = config.ToJson(),
		["Game"] = gameSave
	};

	/// <summary>
	/// Writes to a temp file then moves it over <paramref name="path"/>, so a crash mid-write leaves the
	/// previous save intact rather than a half-written one.
	/// </summary>
	public static void Write( string path, GameConfiguration config, SoloGame game )
		=> Write( path, config, game.Save() );

	/// <summary> For suspending a game that is still running: pass <see cref="SoloGame.LatestCheckpoint"/>. </summary>
	public static void Write( string path, GameConfiguration config, JsonObject gameSave ) {
		string temp = path + ".tmp";
		File.WriteAllText( temp, ToJson( config, gameSave ).ToJsonString() );
		File.Move( temp, path, overwrite: true );
	}

	public static (SoloGame Game, GameConfiguration Config) Read( string path, GameBuilder builder )
		=> FromJson( JsonNode.Parse( File.ReadAllText( path ) )!.AsObject(), builder );

	/// <summary>
	/// Rebuilds the game from the saved configuration and loads the saved state onto it. Also calls
	/// ActionScope.Initialize on the calling execution context (as SoloGame.Start does), so call this from
	/// the context that will drive the game - one live game per context.
	/// </summary>
	public static (SoloGame Game, GameConfiguration Config) FromJson( JsonObject json, GameBuilder builder ) {
		int version = json["Version"]!.GetValue<int>();
		if( version != FormatVersion )
			throw new InvalidOperationException( $"Unsupported save file version {version} (expected {FormatVersion})." );

		GameConfiguration config = GameConfiguration.FromJson( (JsonObject)json["Config"]! );
		GameState gameState = builder.BuildGame( config );
		var game = new SoloGame( gameState );
		ActionScope.Initialize( gameState.RootScope );
		game.Load( (JsonObject)json["Game"]! );
		return (game, config);
	}

}
