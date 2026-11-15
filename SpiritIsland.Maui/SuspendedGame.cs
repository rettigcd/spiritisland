using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Maui;

/// <summary>
/// Keeps the current game on disk while the app is in the background, so it survives the OS closing the app
/// and can be resumed from the main page. One slot: the most recent game.
/// </summary>
static class SuspendedGame {

	static string FilePath => System.IO.Path.Combine( FileSystem.AppDataDirectory, "suspended-game.json" );

	public static bool Exists => File.Exists( FilePath );

	/// <summary> Writes the current game (if there is one worth saving), or removes the file if it is over. </summary>
	public static void Save() {
		SoloGamePage? page = SoloGamePage.Current;
		if( page is null ) return;
		try {
			if( !page.Suspend( FilePath ) && page.IsGameOver )
				Delete();
		} catch( Exception ) {
			// Failing to suspend must never take the app down; the game just can't be resumed.
		}
	}

	public static void Delete() {
		try { File.Delete( FilePath ); } catch( IOException ) { }
	}

	/// <summary>
	/// Rebuilds the saved game and returns a page for it (not started - SoloGamePage starts it). Must be called
	/// on the UI thread, which is where the game will run.
	/// </summary>
	public static SoloGamePage Resume() {
		( SoloGame game, GameConfiguration config ) = SoloGameFile.Read( FilePath, NewGameModel.Builder );
		var model = new SoloGameModel( game, config );
		model.Log.SpecifyGameConfig( NewGameModel.Describe( config ) );
		return new SoloGamePage( model );
	}

}
