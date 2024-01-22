using SpiritIsland.Log;

namespace SpiritIsland;

public class DecisionLogEntry( IOption _selection, IDecision _decision, bool _auto ) : ILogEntry {
	public string Msg( LogLevel level ) {
		// Fatal/Error/Warn/Info
		if( level <= LogLevel.Info)
			return _decision.Prompt + ": " + _selection.Text;

		// Debug/All
		string msg = _decision.Prompt + "(" + _decision.Options.Select( o => o.Text ).Join( "," ) + "):" + _selection.Text;
		if(_auto) msg += " AUTO";
		return msg;
	}

	public LogLevel Level => LogLevel.Info;
}
