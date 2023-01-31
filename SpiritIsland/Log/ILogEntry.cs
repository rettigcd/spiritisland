namespace SpiritIsland.Log;

public interface ILogEntry {
	string Msg( LogLevel level );
	LogLevel Level { get; }
}

public enum LogLevel {
	None,   // shows least # of messages (0)
	Fatal,  // shows Fatal (only)
	Error,  // shows Fatal & Error
	Warning,// shows Fatal & Error & Warning
	Info,   // shows Fatal & Error & Warning & Info
	Debug,  // shows Fatal & Error & Warning & Info & Debug
	All     // show  all
}
