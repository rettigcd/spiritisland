namespace SpiritIsland;

public interface ILogEntry {
	string Msg( LogLevel level );
	LogLevel Level { get; }
}

static public class LogEntryExtension {
	static public string Msg(this ILogEntry logEntry) => logEntry.Msg(LogLevel.Info);
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

public class IslandBlighted : ILogEntry { // event
	public IslandBlighted( IBlightCard card ) {
		this.Level = LogLevel.Info;
		this.card = card;
	}
	public LogLevel Level { get; }
	public IBlightCard card { get; }
	public string Msg( LogLevel _ ) => $"Blighted Island => {card.Name} => {card.Immediately.Description}\r\n  ^^^^^^^^ ^^^^^^\r\n";
}

public class InvaderActionEntry : ILogEntry {
	public string msg;
	public InvaderActionEntry( string msg, LogLevel level = LogLevel.Info ) { 
		this.msg = msg;
		this.Level = level;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => msg;
}
public class SpaceExplored : InvaderActionEntry {
	public SpaceExplored( Space space ):base( space + ":gains explorer" ) { Space = space; }
	public Space Space { get; }
}

public class LogRound : ILogEntry {
	public int round;
	public LogRound( int round ) { 
		this.round = round;
		this.Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => $"=== Round {round} ===";
}

public class LogPhase : ILogEntry {
	public Phase phase;
	public LogPhase( Phase phase ) { 
		this.phase = phase;
		this.Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => $"-- {phase} --";
}

public class LogException : ILogEntry {
	public System.Exception ex;
	public LogException( System.Exception ex ) { 
		this.ex = ex;
		this.Level = LogLevel.Fatal;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => ex.ToString();
}

public class LogDebug : ILogEntry {
	public LogDebug( string text ) { this.text = text; }
	readonly string text;
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel _ ) => text;
}
