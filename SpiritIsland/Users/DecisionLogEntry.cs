using SpiritIsland.Log;

namespace SpiritIsland;

public class DecisionLogEntry : ILogEntry {

	readonly IOption selection;
	readonly IDecision decision;
	readonly bool auto;

	public DecisionLogEntry(IOption selection, IDecision decision, bool auto ) {
		this.selection = selection;
		this.decision = decision;
		this.auto = auto;
	}

	public string Msg( LogLevel level ) {
		// Fatal/Error/Warn/Info
		if( level <= LogLevel.Info)
			return decision.Prompt + ": " + selection.Text;

		// Debug/All
		string msg = decision.Prompt + "(" + decision.Options.Select( o => o.Text ).Join( "," ) + "):" + selection.Text;
		if(auto) msg += " AUTO";
		return msg;
	}

	public LogLevel Level => LogLevel.Info;
}
