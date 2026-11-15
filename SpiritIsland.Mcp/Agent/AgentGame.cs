using System.Text.Json;
using System.Text.Json.Nodes;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Agent;

/// <summary>
/// Drives one single-spirit game without a UI: exposes the pending decision as a snapshot and applies
/// a choice by option text. Rewind and withdraw are deliberately not exposed.
/// The game can be written to a <see cref="Save"/> and rebuilt with <see cref="Restore"/>, so a host needn't keep the
/// engine's async task alive between calls: it restores, makes its calls, saves, and drops the instance (the engine task
/// is left waiting on a decision, holds no thread, and is collected with the instance).
/// Only one AgentGame may be driven at a time per execution context (ActionScope is ambient).
/// </summary>
public sealed class AgentGame {

	public NewGameRequest Request { get; }
	public GameState GameState => _game.GameState;
	public List<string> History { get; } = [];

	public AgentGame( NewGameRequest request ) : this( request, null ) { }

	/// <param name="checkpoint">A <see cref="SoloGame.LatestCheckpoint"/> to continue from instead of starting a new game.</param>
	AgentGame( NewGameRequest request, JsonObject? checkpoint ) {
		Request = request;
		_game = new SoloGame( GameSetup.Build( request ) ) {
			EnablePreselects = false, // keep each decision a plain list of options
			LogExceptions = true,
		};
		_game.GameState.NewLogEntry += _alertBuffer.Add; // before Start, so setup entries aren't missed
		if( checkpoint is not null ) {
			_startingCards = [.. GameState.Spirits.Single().Hand]; // as built, before the saved hand replaces it
			ActionScope.Initialize( _game.GameState.RootScope );
			_game.Load( checkpoint ); // Start continues from the saved step: the first decision is the menu it was saved at
		}
		_game.Start();
	}

	#region Save / Restore

	/// <summary>
	/// Everything needed to <see cref="Restore"/> this game: the checkpoint at the last action boundary plus the picks made
	/// since (the engine is deterministic, so replaying them reaches the same decision), and what the agent has been shown
	/// (version, alerts, observation history). Settles on the pending decision first, so call it between tool calls.
	/// </summary>
	public JsonObject Save() {
		GetDecision();
		return new JsonObject {
			["Request"] = JsonSerializer.SerializeToNode( Request ),
			["AutoSelect"] = AutoSelectSingleOption,
			["Checkpoint"] = _baseline?.DeepClone(),
			["Picks"] = new JsonArray( [.. _picks.Select( p => (JsonNode)p )] ),
			["Over"] = IsOver,
			["Version"] = _version,
			["History"] = new JsonArray( [.. History.Select( p => (JsonNode)p )] ),
			["PendingAlerts"] = AlertsJson( _pendingAlerts ),
			["FinalAlerts"] = AlertsJson( FinalAlerts ),
			["AutoPicks"] = new JsonArray( [.. _autoPicksSinceSelect.Select( a => (JsonNode)new JsonObject { ["Prompt"] = a.Prompt, ["Option"] = a.Option } )] ),
			["Observations"] = Observations.ToJson(),
		};
	}

	/// <summary> Rebuilds a game from a <see cref="Save"/>, waiting at the decision it was saved at. </summary>
	public static AgentGame Restore( JsonObject save ) {
		var request = save["Request"].Deserialize<NewGameRequest>()!;
		var game = new AgentGame( request, save["Checkpoint"]?.AsObject() ) { AutoSelectSingleOption = save["AutoSelect"]!.GetValue<bool>() };
		foreach( JsonNode? pick in save["Picks"]!.AsArray() ) {
			game.GetDecision();
			game.Select( game._version, pick!.GetValue<string>() );
		}
		game.GetDecision(); // settle, so nothing the replay produced is left to be reported later
		if( game.IsOver != save["Over"]!.GetValue<bool>() ) // the engine is deterministic, so this means a bad save or a changed engine
			throw new InvalidOperationException( "Restoring the saved game " + (game.IsOver ? "ended it: " + string.Join( "; ", game.FinalAlerts.Select( a => a.Text ) ) : "did not end it, but it was saved ended.") );

		// What the agent saw, as it was: replaying regenerated it, but not what happened before the checkpoint.
		game._version = save["Version"]!.GetValue<int>();
		game.History.Clear();
		game.History.AddRange( save["History"]!.AsArray().Select( n => n!.GetValue<string>() ) );
		game._pendingAlerts = ReadAlerts( save["PendingAlerts"] );
		game.FinalAlerts = ReadAlerts( save["FinalAlerts"] );
		game._carriedAlerts.Clear();
		game._alertBuffer.Drain();
		game._autoPicksSinceSelect.Clear();
		game._autoPicksSinceSelect.AddRange( save["AutoPicks"]!.AsArray().Select( n => (n!["Prompt"]!.GetValue<string>(), n["Option"]!.GetValue<string>()) ) );
		game.Observations.Restore( save["Observations"]!.AsObject() );
		return game;
	}

	static JsonArray AlertsJson( IEnumerable<Alert> alerts )
		=> new( [.. alerts.Select( a => (JsonNode)new JsonObject { ["kind"] = a.Kind, ["text"] = a.Text } )] );

	static Alert[] ReadAlerts( JsonNode? node )
		=> [.. node!.AsArray().Select( n => new Alert( n!["kind"]!.GetValue<string>(), n["text"]!.GetValue<string>() ) )];

	/// <summary>
	/// Keeps <see cref="_baseline"/> at the engine's newest action-boundary checkpoint, dropping the picks it already
	/// includes. Called each time the game settles on a decision (or ends): every pick since the previous call was made
	/// after the baseline it replaces, or is part of the actions that led up to the new one.
	/// </summary>
	void SyncBaseline() {
		if( _baselineVersion == _game.CheckpointVersion ) return;
		_baselineVersion = _game.CheckpointVersion;
		_baseline = _game.LatestCheckpoint;
		_picks.Clear();
	}

	JsonObject? _baseline;
	int _baselineVersion;
	readonly List<string> _picks = []; // agent picks made since _baseline, in order

	#endregion Save / Restore

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

	/// <summary> The last observations sent to the agent, so the next can be sent as a diff. </summary>
	public ObservationHistory Observations { get; } = new();

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
					_autoPicksSinceSelect.Add( (next.Prompt, only) );
					portal.Choose( next, next.Options[0] );
					continue;
				}
				_pending = next;
				SyncBaseline();
				_version++;
				_pendingAlerts = [.. _carriedAlerts]; // kept with the decision so repeated reads show the same alerts
				_carriedAlerts.Clear();
			}
			if( _pending is not null )
				return Snapshot( _pending );
			if( IsOver ) {
				SyncBaseline();
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
		WarnIfDoneSkipsPowers( decision, option );
		_pending = null;
		_autoPicksSinceSelect.Clear();
		History.Add( optionText );
		_picks.Add( optionText );
		_game.UserPortal.DecisionPortal.Choose( decision, option );
	}

	/// <summary>
	/// Choosing "Done" on "Select Fast/Slow to resolve" ends that phase's resolving, so any power still on offer that
	/// can't resolve later this turn is dropped. That's occasionally deliberate, but has also lost games by accident,
	/// so say which ones with a "skipped" alert. In the Fast phase, a power that can still resolve in the Slow phase
	/// (e.g. a Slow card offered early by Lightning's Swiftness) isn't lost, so it isn't listed.
	/// </summary>
	void WarnIfDoneSkipsPowers( IDecision decision, IOption chosen ) {
		Phase phase = GameState.Phase;
		if( chosen is IActionFactory || phase is not (Phase.Fast or Phase.Slow)
			|| !decision.Prompt.StartsWith( $"Select {phase} to resolve" ) ) return;
		ActionScope.Initialize( GameState.RootScope ); // engine state is read through AsyncLocal scope
		Spirit spirit = GameState.Spirits.Single();
		string[] lost = [.. decision.Options.OfType<IActionFactory>()
			.Where( f => phase == Phase.Slow || !f.CouldActivateDuring( Phase.Slow, spirit ) )
			.Select( f => f.Text )];
		if( lost.Length > 0 )
			_carriedAlerts.Add( new Alert( "skipped", $"'{chosen.Text}' left {phase} powers unresolved (they won't resolve this turn): {string.Join( ", ", lost )}" ) );
	}

	/// <summary>
	/// Applies several choices in order, starting with the decision of the given version. Stops (without throwing) at the
	/// first step whose option isn't offered or whose expected prompt doesn't match, or when the game ends, so a plan that
	/// meets an unexpected prompt (e.g. a fear card's) never answers the wrong question.
	/// A step that names an option the server just auto-selected (the only option) counts as done.
	/// The decision reached afterwards carries the alerts of every decision the batch passed through, plus one
	/// "batch" alert per step saying which prompt it answered.
	/// </summary>
	/// <exception cref="ArgumentException">the version is stale</exception>
	/// <exception cref="InvalidOperationException">the game is already over</exception>
	public BatchResult SelectBatch( int version, IReadOnlyList<BatchStep> steps ) {
		DecisionSnapshot? d = GetDecision() ?? throw new InvalidOperationException( "Game is over." );
		if( version != _version )
			throw new ArgumentException( $"Stale decision version {version}; current is {_version}." );

		var alerts = new List<Alert>();
		int applied = 0;
		string? stoppedBecause = null;
		foreach( BatchStep step in steps ) {
			if( d is null ) { stoppedBecause = "the game is over"; break; }

			bool promptOk = step.Prompt is null || d.Prompt.StartsWith( step.Prompt, StringComparison.OrdinalIgnoreCase );
			if( !promptOk || !d.Options.Contains( step.Option ) ) {
				// Already answered for us because it was the only option? Then the step is done.
				int auto = _autoPicksSinceSelect.FindIndex( a => a.Option == step.Option
					&& (step.Prompt is null || a.Prompt.StartsWith( step.Prompt, StringComparison.OrdinalIgnoreCase )) );
				if( auto >= 0 ) {
					_autoPicksSinceSelect.RemoveAt( auto );
					++applied;
					continue;
				}
				stoppedBecause = !promptOk
					? $"step {applied + 1} expected a prompt starting '{step.Prompt}' but the prompt is '{d.Prompt}'"
					: $"step {applied + 1}: '{step.Option}' is not offered by '{d.Prompt}'";
				break;
			}

			alerts.Add( new Alert( "batch", $"Selected '{step.Option}' for '{d.Prompt}'" ) );
			Select( d.Version, step.Option );
			++applied;
			d = GetDecision();
			alerts.AddRange( d?.Alerts ?? [] );
		}

		if( alerts.Count > 0 ) { // something was actually selected (a skipped auto-pick changes nothing)
			if( d is not null ) _pendingAlerts = [.. alerts]; // the last decision's own alerts are already at the end
			else FinalAlerts = [.. alerts, .. FinalAlerts];
		}
		return new BatchResult( applied, steps.Count, stoppedBecause );
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
	readonly List<Alert> _carriedAlerts = [];
	readonly List<(string Prompt, string Option)> _autoPicksSinceSelect = []; // lets a batch step that names an auto-selected option count as done // alerts (incl. auto-selections) waiting for the next decision the agent sees
	int _version;
	readonly AlertBuffer _alertBuffer = new();
	readonly SoloGame _game;
}
