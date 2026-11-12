namespace SpiritIsland.Log;

/// <summary> A Spirit lost Presence from the island (destroyed, or removed/replaced by an effect). </summary>
public class PresenceDestroyed( string _spiritName, string _from, int _count, RemoveReason _reason ) : ILogEntry {
	public LogLevel Level { get; } = LogLevel.Info;
	public string Msg( LogLevel _ ) => $"{_spiritName}: {_count} Presence {Verb} from {_from}.";

	string Verb => _reason switch {
		RemoveReason.Replaced => "replaced",
		RemoveReason.Removed => "removed",
		_ => "destroyed",
	};
}
