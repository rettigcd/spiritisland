using System.Text.Json.Nodes;
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
		_game.GameState.NewLogEntry += _alertBuffer.Add; // before Start, so setup entries aren't missed
		_game.Start();
	}

	public bool IsOver => _game.EngineTask!.IsCompleted;

	/// <summary> Result if the game ended; null while in progress (or if the engine faulted - see <see cref="Error"/>). </summary>
	public GameOverLogEntry? Result => IsOver ? GameState.Result : null;

	/// <summary> Set if the engine task ended without a win/loss result. </summary>
	public string? Error => IsOver && GameState.Result is null ? "Engine stopped without a result (see log)." : null;

	/// <summary>
	/// When true (the default), a decision with exactly one option is chosen automatically and reported as an
	/// "auto" alert on the next decision the agent sees, saving a round trip for a choice that isn't one.
	/// </summary>
	public bool AutoSelectSingleOption { get; init; } = true;

	/// <summary> The cards in the spirit's hand when the first decision was asked. </summary>
	public IReadOnlyList<PowerCard> StartingCards {
		get { if( _startingCards is null ) GetDecision(); return _startingCards ?? []; }
	}
	PowerCard[]? _startingCards;

	/// <summary> Alerts recorded after the last decision, available once the game is over (when <see cref="GetDecision"/> returns null). </summary>
	public Alert[] FinalAlerts { get; private set; } = [];

	/// <summary> Waits for the engine to present the next decision, or finish. Null when the game is over. </summary>
	public DecisionSnapshot? GetDecision() {
		IDecisionPortal portal = _game.UserPortal.DecisionPortal;
		while( true ) {
			if( _pending is null && portal.WaitForNext( 20 ) ) {
				IDecision next = portal.Next;
				_startingCards ??= [.. GameState.Spirits.Single().Hand]; // setup is done once the first decision is asked
				_carriedAlerts.AddRange( _alertBuffer.Drain() );
				if( AutoSelectSingleOption && next.Options.Length == 1 ) {
					string only = OptionIds( next )[0];
					_carriedAlerts.Add( new Alert( "auto", $"Auto-selected '{only}' (only option for '{next.Prompt}')" ) );
					History.Add( only );
					portal.Choose( next, next.Options[0] );
					continue;
				}
				_pending = next;
				_version++;
				_pendingAlerts = [.. _carriedAlerts]; // kept with the decision so repeated reads show the same alerts
				_carriedAlerts.Clear();
			}
			if( _pending is not null )
				return Snapshot( _pending );
			if( IsOver ) {
				_carriedAlerts.AddRange( _alertBuffer.Drain() );
				FinalAlerts = [.. FinalAlerts, .. _carriedAlerts];
				_carriedAlerts.Clear();
				return null;
			}
		}
	}

	/// <summary>
	/// Cards the player has legitimately seen: hand, cards in play, discard pile, and the cards offered by the pending decision.
	/// Deliberately not the power decks (hidden information).
	/// </summary>
	public IEnumerable<(string Location, PowerCard Card)> KnownCards() {
		Spirit spirit = GameState.Spirits.Single();
		foreach( PowerCard c in spirit.Hand ) yield return ("hand", c);
		foreach( PowerCard c in spirit.InPlay ) yield return ("inPlay", c);
		foreach( PowerCard c in spirit.DiscardPile ) yield return ("discard", c);
		if( _pending is not null )
			foreach( PowerCard c in _pending.Options.OfType<PowerCard>() ) yield return ("offered", c);
	}

	DecisionSnapshot Snapshot( IDecision decision ) {
		string[] ids = OptionIds( decision );
		return new DecisionSnapshot( _version, decision.Prompt, ids, _pendingAlerts, OptionDetails( decision, ids ), PromptDetails.For( decision.Prompt ) );
	}

	/// <summary>
	/// Choosing between cards: describe each one (elements, range, target, instructions) and what happens to the chosen card.
	/// Not for choosing which hand card to play - those are already in the observation, and the prompt repeats every card played.
	/// </summary>
	static Dictionary<string, JsonObject> OptionDetails( IDecision decision, string[] ids ) {
		var details = new Dictionary<string, JsonObject>();
		if( decision is not A.PowerCard cardDecision ) return details;
		for( int i = 0; i < ids.Length; ++i )
			if( decision.Options[i] is PowerCard card && cardDecision.Use( card ) != CardUse.Play ) {
				JsonObject json = PlayerObservation.CardJson( card );
				json["use"] = cardDecision.Use( card ).ToString();
				details[ids[i]] = json;
			}
		return details;
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
	Alert[] _pendingAlerts = [];
	readonly List<Alert> _carriedAlerts = []; // alerts (incl. auto-selections) waiting for the next decision the agent sees
	int _version;
	readonly AlertBuffer _alertBuffer = new();
	readonly SoloGame _game;
}
