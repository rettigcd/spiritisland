using SpiritIsland.Log;

namespace SpiritIsland;

public class DecisionLogEntry : ILogEntry {

	public DecisionLogEntry(IOption selection, IDecision decision, bool auto) {
		ArgumentNullException.ThrowIfNull(selection,nameof(selection));
		ArgumentNullException.ThrowIfNull(decision,nameof(decision));
		_selection = selection;
		_decision = decision;
		_auto = auto;
	}

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


	readonly IOption _selection;
	readonly IDecision _decision;
	readonly bool _auto;
}
