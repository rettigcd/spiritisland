using SpiritIsland.Log;

namespace SpiritIsland.Web.View;

/// <summary>
/// Which of the engine's log entries a player is shown, and how they are grouped. The groups are the Maui log's, and the
/// MCP server's alerts use the same ones.
/// </summary>
public static class LogFilter {

	/// <summary> The line to show for an entry; null for one that is not shown. </summary>
	public static LogLineView? From( ILogEntry entry ) {
		if( entry is DecisionLogEntry or ExceptionEntry ) return null; // what you chose you already know; a failure is reported as an error
		if( Log.LogLevel.Info < entry.Level ) return null; // the Debug detail (each token moved, ...) is on the board
		return new LogLineView( KindOf( entry ), Clean( entry.Msg( Log.LogLevel.Info ) ) );
	}

	/// <summary> The engine writes for a console: some messages underline words on a second line of ^ marks. Drop those and the blank ends. </summary>
	static string Clean( string text ) => string.Join( "\n", text.Split( ["\r\n", "\n"], StringSplitOptions.None )
		.Where( line => !( line.Contains( '^' ) && line.All( c => c == '^' || char.IsWhiteSpace( c ) ) ) )
		.Select( line => line.TrimEnd() ) )
		.Trim();

	static string KindOf( ILogEntry entry ) => entry switch {
		GameOverLogEntry or IslandBlighted or PresenceDestroyed => "bad",
		InvaderActionEntry or RavageEntry or SpaceExplored => "invaders",
		Log.Phase or Round => "phase",
		BlightOnCardChanged or FearCardRevealed or FearGenerated => "fear-blight",
		_ => "other",
	};
}
