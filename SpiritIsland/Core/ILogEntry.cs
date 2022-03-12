namespace SpiritIsland;

public interface ILogEntry {
	string Msg {  get; }
	LogLevel Level { get; }
}

public enum LogLevel { None, Fatal, Error, Warning, Info, Debug, All }

// Generic
public class LogEntry : ILogEntry {
	public string msg;
	public LogEntry( string msg, LogLevel level = LogLevel.Info ) { 
		this.msg = msg;
		this.Level = level;
	}
	public LogLevel Level { get; }
	public string Msg => msg;
}

public class IslandBlighted : ILogEntry { // event
	public IslandBlighted( IBlightCard card ) {
		this.Level = LogLevel.Info;
		this.card = card;
	}
	public LogLevel Level { get; }
	public IBlightCard card { get; }
	public string Msg => $"Blighted Island => {card.Name} => {card.Immediately.Description}\r\n  ^^^^^^^^ ^^^^^^\r\n";
}


public class InvaderActionEntry : ILogEntry {
	public string msg;
	public InvaderActionEntry( string msg, LogLevel level = LogLevel.Info ) { 
		this.msg = msg;
		this.Level = level;
	}
	public LogLevel Level { get; }
	public string Msg => msg;
}

public class LogRound : ILogEntry {
	public int round;
	public LogRound( int round ) { 
		this.round = round;
		this.Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg => $"=== Round {round} ===";
}

public class LogPhase : ILogEntry {
	public Phase phase;
	public LogPhase( Phase phase ) { 
		this.phase = phase;
		this.Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg => $"-- {phase} --";
}

public class LogException : ILogEntry {
	public System.Exception ex;
	public LogException( System.Exception ex ) { 
		this.ex = ex;
		this.Level = LogLevel.Fatal;
	}
	public LogLevel Level { get; }
	public string Msg => ex.ToString();
}
