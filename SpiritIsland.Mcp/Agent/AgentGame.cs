using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Agent;

/// <summary>
/// Drives one single-spirit game without a UI: exposes the pending decision as a snapshot and applies
/// a choice by option text. Rewind and withdraw are deliberately not exposed.
/// NOT safe to run alongside another AgentGame (the engine's ActionScope/static state is untested for that).
/// </summary>
public sealed class AgentGame {

	public NewGameRequest Request { get; }
	public GameState GameState => _game.GameState;
	public List<string> History { get; } = [];

	public AgentGame( NewGameRequest request ) {
		Request = request;
		_game = new SoloGame( GameSetup.Build( request ) ) {
			EnablePreselects = false, // keep each decision a plain list of options
			LogExceptions = true,
		};
		_game.Start();
	}

	public bool IsOver => _game.EngineTask!.IsCompleted;

	/// <summary> Result if the game ended; null while in progress (or if the engine faulted - see <see cref="Error"/>). </summary>
	public GameOverLogEntry? Result => IsOver ? GameState.Result : null;

	/// <summary> Set if the engine task ended without a win/loss result. </summary>
	public string? Error => IsOver && GameState.Result is null ? "Engine stopped without a result (see log)." : null;

	/// <summary> Waits for the engine to present the next decision, or finish. Null when the game is over. </summary>
	public DecisionSnapshot? GetDecision() {
		IDecisionPortal portal = _game.UserPortal.DecisionPortal;
		while( true ) {
			if( _pending is null && portal.WaitForNext( 20 ) ) {
				_pending = portal.Next;
				_version++;
			}
			if( _pending is not null )
				return new DecisionSnapshot( _version, _pending.Prompt, OptionIds( _pending ) );
			if( IsOver ) return null;
		}
	}

	/// <summary> Applies the option with the given text to the decision with the given version. </summary>
	public void Select( int version, string optionText ) {
		if( GetDecision() is null ) throw new InvalidOperationException( "Game is over." );
		if( version != _version )
			throw new ArgumentException( $"Stale decision version {version}; current is {_version}." );
		int index = Array.IndexOf( OptionIds( _pending! ), optionText );
		if( index < 0 ) throw new ArgumentException( $"'{optionText}' is not an offered option." );
		IOption option = _pending!.Options[index];

		IDecision decision = _pending;
		_pending = null;
		History.Add( optionText );
		_game.UserPortal.DecisionPortal.Choose( decision, option );
	}

	/// <summary>
	/// Option text is the ID. The engine intends it to be unique but doesn't always achieve it, so:
	/// (1) Growth options are labelled with the group they belong to (the UI conveys this by layout), and
	/// (2) any remaining repeats get a ' [n]' suffix (n = occurrence, from 2).
	/// Both only change what the agent sees; selection maps back to the engine's option object by position.
	/// </summary>
	string[] OptionIds( IDecision decision ) {
		var seen = new Dictionary<string, int>();
		return [.. decision.Options.Select( o => {
			string text = decision is A.GrowthDecision ? GrowthLabel( o ) : o.Text;
			int n = seen[text] = seen.GetValueOrDefault( text ) + 1;
			return n == 1 ? text : $"{text} [{n}]";
		} )];
	}

	string GrowthLabel( IOption option ) {
		GrowthGroup? group = GameState.Spirits.Single().GrowthTrack.Groups
			.FirstOrDefault( g => g.GrowthActionFactories.Contains( option ) );
		return group is null || group.GrowthActionFactories.Length < 2 ? option.Text
			: $"{option.Text} (group: {group})";
	}

	IDecision? _pending;
	int _version;
	readonly SoloGame _game;
}
