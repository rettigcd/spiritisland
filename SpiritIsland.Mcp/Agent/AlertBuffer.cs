using SpiritIsland.Log;

namespace SpiritIsland.Agent;

/// <summary>
/// Collects the engine's log entries (raised from the engine's thread) so they can be reported with the next decision.
/// Decisions made are left out - the agent already knows what it chose.
/// </summary>
sealed class AlertBuffer {

	public void Add( ILogEntry entry ) {
		if( entry is DecisionLogEntry ) return;
		var alert = new Alert( KindOf( entry ), entry.Msg( LogLevel.Info ) );
		lock( _lock ) _alerts.Add( alert );
	}

	/// <summary> Returns what was collected since the last call and starts over. </summary>
	public Alert[] Drain() {
		lock( _lock ) {
			Alert[] result = [.. _alerts];
			_alerts.Clear();
			return result;
		}
	}

	// Same groups as the Maui LogModel's colors.
	static string KindOf( ILogEntry entry ) => entry switch {
		GameOverLogEntry or ExceptionEntry or IslandBlighted => "bad",
		InvaderActionEntry or RavageEntry or SpaceExplored => "invaders",
		RewindException => "rewind",
		Log.Phase or Round => "phase",
		BlightOnCardChanged or FearCardRevealed or FearGenerated => "fear-blight",
		Debug or LayoutChanged or TokenMovedArgs or TokenReplacedArgs or HumanAdjustment => "tokens",
		_ => "other",
	};

	readonly List<Alert> _alerts = [];
	readonly object _lock = new();
}
