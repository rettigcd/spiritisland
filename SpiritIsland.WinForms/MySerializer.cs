using System;
using System.Linq;
using Outfish.JavaScript;
using System.Collections.Generic;
using System.IO;

namespace SpiritIsland.WinForms;

// == With Dependency Options  ==
// * The individual classes use the Serializer and do the whole tree
// * Each class that is serializeable returns / consumes a Serializeable object

// == WITHOUT Dependency Options == 
// * how to convert each item to a Dictionary is known by the Serializer - via registering?
// * use Reflection via System.Text.Json

public static class MySerializer {

	#region File I/O

	static public void Add( GameConfigPlusToken cfg ) {
		const int MAXGAMES = 20;

		var recentGames = GetRecent().ToList();

		// Find matching game using TIMESTAMP - so don't update it until this match is complete.
		var match = recentGames.Where( g => g.TimeStamp == cfg.TimeStamp ).FirstOrDefault();
		// If we are reloading an old saved game config, swap out this cfg, for the pre-existing match.
		if( match != null)
			cfg = match;
		else
			recentGames.Add(cfg);

		// Now that we have (possibly) swapped out for a pre-existing saved configuration
		// Now it is safe to set the time stamp on whatever config we are actually going to save.
		cfg.TimeStamp = StripPartialSeconds(DateTime.Now); // remove partial seconds because not serializing that.

		// order and crop	
		var prepped = recentGames
			.OrderByDescending( x => x.TimeStamp )
			.Take( MAXGAMES )
			.Select( Prepare )
			.ToArray();

		// save
		File.WriteAllText( GetSettingsFileName(), Json.Serialize( prepped, 1 ) );
	}

	static DateTime StripPartialSeconds(DateTime dt) {
		dt.AddTicks( -(dt.Ticks % TimeSpan.TicksPerSecond) );
		return dt;
	}

	static public GameConfigPlusToken[] GetRecent() {
		string settingsFile = GetSettingsFileName();
		return File.Exists(settingsFile)
			? Json.DeserializeArray( File.ReadAllText( settingsFile ) )
				.Select( x=> RestoreGameConfigPlusToken((JsonObject)x) )
				.ToArray()
			: [];
	}

	static string GetSettingsFileName() 
		=> Path.Combine( AppDataFolder.GetRootPath(), "settings.json" );

	#endregion File I/O

	#region GameConfig
	// Game Config (with Presence Token)
	static public Dictionary<string, object> Prepare( GameConfigPlusToken item ) {
		if(item is null) return null;

		var result = new Dictionary<string, object> {
			[SPIRIT] = item.Spirits[0], // !!! change format to save >1 spirits/boards
			[BOARD] = item.Boards[0],
			[SHUFFLENUMBER] = item.ShuffleNumber,
			[ADVERSARY] = Prepare( item.Adversary ),
			[TIMESTAMP] = Prepare( item.TimeStamp )
		};
		return result;
	}

	static public GameConfigPlusToken RestoreGameConfigPlusToken( JsonObject dict ) => dict is null ? null
		: new GameConfigPlusToken { 
			Spirits = [ dict[SPIRIT] ],
			Boards = [ dict[BOARD] ],
			ShuffleNumber = (int)dict[SHUFFLENUMBER],
			Adversary = RestoreAdversaryConfig( dict[ADVERSARY] ), 
			TimeStamp = RestoreDateTime( dict[TIMESTAMP] )
		};
	#endregion Game Config

	#region DateTime
	const string DATETIME_FORMAT = "yyyy-MMM-dd HH:mm:ss";
	static public string Prepare( DateTime? dt) => dt?.ToString(DATETIME_FORMAT);
	static public DateTime? RestoreDateTime( string s ) => s is null ? null 
		: DateTime.ParseExact( s, DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture );
	#endregion DateTime

	#region AdversayConfig
	static public Dictionary<string, object> Prepare( AdversaryConfig item ) => item is null ? null 
		: new Dictionary<string, object>() { [NAME] = item.Name, [LEVEL] = item.Level };
	static public AdversaryConfig RestoreAdversaryConfig( JsonObject dict ) => dict is null ? null 
		: new AdversaryConfig( (string)dict[NAME], (int)dict[LEVEL] );
	#endregion Adversary

	#region private Keys
	const string SHUFFLENUMBER = "shuffleNumber";
	const string SPIRIT        = "spirit";
	const string BOARD         = "board";
	const string ADVERSARY     = "adversary";
	const string NAME          = "name";
	const string LEVEL         = "level";
	const string TOKEN         = "token";
	const string ADJUST        = "adjust";
	const string HUE           = "hue";
	const string SATURATION    = "saturation";
	const string LIGHTNESS     = "lightness";
	const string IMAGE         = "image";
	const string PATTERN       = "pattern";
	const string TIMESTAMP     = "timestamp";
	#endregion

}
