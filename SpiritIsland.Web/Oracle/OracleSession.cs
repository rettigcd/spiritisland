using System.Text.Json.Nodes;
using SpiritIsland.SinglePlayer;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Oracle;

/// <summary> A pick that isn't one of the offered options, or that comes after the game has ended. </summary>
public sealed class InvalidPickException( string message ) : Exception( message ) { }

/// <summary> The game engine ended without a result: it failed. The engine's own exception is the inner exception, when it logged one. </summary>
public sealed class EngineStoppedException( string message, Exception? inner ) : Exception( message, inner ) { }

/// <summary> What a session reached after replaying picks. </summary>
/// <param name="Checkpoint">The newest action-boundary checkpoint.</param>
/// <param name="CheckpointMoved">True if replaying crossed an action boundary, so <paramref name="Checkpoint"/> is newer than the one started from.</param>
/// <param name="Picks">The picks made since <paramref name="Checkpoint"/>.</param>
/// <param name="LogCompleted">Log lines from before <paramref name="Checkpoint"/> that were not in the record this session was resumed from: the record keeps them.</param>
/// <param name="LogPending">Log lines from after <paramref name="Checkpoint"/>: the action in progress. Replaying the picks makes them again, so the record does not keep them.</param>
public record SessionState( JsonObject Checkpoint, bool CheckpointMoved, string[] Picks, StaticView Static, GameView View, DecisionView? Decision, ResultView? Result,
	LogLineView[] LogCompleted, LogLineView[] LogPending );

/// <summary>
/// Runs one game for the length of one request: loads a checkpoint, replays picks, and reports the decision the game is
/// then waiting on. Nothing outlives the request: the engine is a long-running task that is still waiting on that
/// decision, so before the session ends it is cancelled and left to finish (see <see cref="Cancel"/>).
/// Same pattern as the MCP server's AgentGame, without its agent-specific parts (auto-select, alerts, observation diffs,
/// batches).
/// Only one session may be driven at a time per execution context (ActionScope is ambient), so call these from their own
/// Task.Run.
/// </summary>
public sealed class OracleSession : IDisposable {

	/// <summary> Engines of sessions in progress, plus any a finished session couldn't stop. Zero between requests; a count that keeps growing means cancelling isn't working. </summary>
	public static int EnginesRunning => Volatile.Read( ref _enginesRunning );
	static int _enginesRunning;

	/// <summary> Starts a new game and reaches its first decision. </summary>
	public static SessionState Start( GameConfiguration config ) {
		using var session = new OracleSession( new SoloGame( GameSetup.Builder.BuildGame( config ) ), startedAt: null );
		return session.Replay( [] );
	}

	/// <summary> Loads the checkpoint, replays <paramref name="picks"/> by option text, and reaches the next decision. </summary>
	/// <exception cref="InvalidPickException"/>
	public static SessionState Resume( GameRecord record, IReadOnlyList<string> picks ) {
		(SoloGame game, _) = SoloGameFile.FromJson( record.ToJson(), GameSetup.Builder );
		using var session = new OracleSession( game, startedAt: record.Checkpoint );
		return session.Replay( picks );
	}

	OracleSession( SoloGame game, JsonObject? startedAt ) {
		_game = game;
		_game.LogExceptions = true;
		// Actions are offered together with what they target, so the client can show everything a power could hit and let the
		// player pick the power and its target in one go. Set on every replay, because picks are matched by option text.
		_game.EnableCombinedActions = true;
		_baseline = startedAt;
		_resumed = startedAt is not null;
		_game.GameState.NewLogEntry += entry => { // before Start, so none is missed. Runs on the engine's thread.
			if( entry is Log.ExceptionEntry failure ) _failure ??= failure.Ex;
			if( LogFilter.From( entry ) is { } line )
				lock( _log ) _log.Add( (_game.CheckpointVersion, line) ); // which checkpoint it came after, to tell what is before the newest one
		};
		_game.Start();
		Interlocked.Increment( ref _enginesRunning );
	}

	public void Dispose() {
		Cancel();
		if( _game.EngineTask!.IsCompleted ) // counted out here, not from a callback, so the count is exact the moment a request ends
			Interlocked.Decrement( ref _enginesRunning );
	}

	/// <summary>
	/// Ends the engine task, which is waiting on a decision nobody is going to answer. A withdrawal (the engine's own way
	/// to cancel a game) is thrown into that decision, and the engine unwinds and finishes. This all happens on the calling
	/// thread. It loops in case some engine code catches the exception and carries on to its next decision (none does today).
	/// The game state is thrown away, so the withdrawal's result is never read.
	/// </summary>
	void Cancel() {
		IDecisionPortal portal = _game.UserPortal.DecisionPortal;
		for( int attempt = 0; attempt < 50 && !_game.EngineTask!.IsCompleted; ++attempt )
			if( portal.WaitForNext( 100 ) ) // never ask without a decision waiting: it would block until one arrived
				_game.UserPortal.CancelGame();
	}

	SessionState Replay( IReadOnlyList<string> picks ) {
		bool moved = false;
		DecisionView? decision = Settle( ref moved, firstLook: true );
		// A resumed game starts at a checkpoint whose earlier log the record already has.
		if( _resumed ) lock( _log ) _log.RemoveAll( l => l.Version < _baselineVersion );
		foreach( string pick in picks ) {
			if( decision is null ) {
				ThrowIfEngineFailed();
				throw new InvalidPickException( $"Cannot pick '{pick}': the game is over." );
			}
			Select( pick );
			decision = Settle( ref moved );
		}
		if( decision is null ) ThrowIfEngineFailed();
		LogLineView[] completed, pending;
		lock( _log ) {
			completed = [.. _log.Where( l => l.Version < _baselineVersion ).Select( l => l.Line )];
			pending = [.. _log.Where( l => _baselineVersion <= l.Version ).Select( l => l.Line )];
		}
		return new SessionState( _baseline!, moved, [.. _picksSinceBaseline], StaticViewBuilder.Build( _game.GameState ), GameViewBuilder.Build( _game.GameState, _game.Spirit ),
			decision, Result(), completed, pending );
	}

	/// <summary>
	/// Waits for the engine to present the next decision, or finish (null). Keeps the baseline at the newest action-boundary
	/// checkpoint, dropping the picks it already includes: the picks made since the previous call were either made after the
	/// baseline it replaces or are the actions that led up to the new one.
	/// </summary>
	DecisionView? Settle( ref bool moved, bool firstLook = false ) {
		IDecisionPortal portal = _game.UserPortal.DecisionPortal;
		long giveUpAt = Environment.TickCount64 + 30_000;
		while( true ) {
			if( _pending is null && portal.WaitForNext( 20 ) ) {
				_pending = portal.Next;
				SyncBaseline( ref moved, firstLook );
			}
			if( _pending is not null )
				return new DecisionView( _pending.Prompt, [.. OptionIds( _pending ).Zip( _pending.Options, ( id, o ) => new OptionView( id, o.Text, OptionTargets.For( o, _game.Spirit ) with { Use = CardUseOf( _pending, o ) }, CardOf( o ) ) )],
					PromptDetails.For( _pending.Prompt ) );
			if( _game.EngineTask!.IsCompleted ) {
				SyncBaseline( ref moved, firstLook );
				return null;
			}
			if( Environment.TickCount64 > giveUpAt )
				throw new TimeoutException( "The game engine did not reach a decision within 30 seconds." );
		}
	}

	/// <summary> What choosing a card does with it (play, add to hand, reclaim, forget, discard...), when the decision is a choice of cards. </summary>
	static string? CardUseOf( IDecision decision, IOption option )
		=> decision is SpiritIsland.A.PowerCard cards && option is PowerCard card && cards.CardOptions.Contains( card ) ? cards.Use( card ).ToString() : null;

	/// <summary> The card's details, for an option that is a card (or a card together with what it targets). </summary>
	static CardView? CardOf( IOption option )
		=> ( option is TargetedAction act ? act.Action : option ) is PowerCard card ? GameViewBuilder.CardOf( card ) : null;

	/// <summary> A game that is over has a result. An engine that ended without one stopped because something went wrong. </summary>
	void ThrowIfEngineFailed() {
		if( _game.GameState.Result is null )
			throw new EngineStoppedException( "The game engine stopped without finishing the game" + (_failure is null ? "." : ": " + _failure.Message), _failure );
	}

	void SyncBaseline( ref bool moved, bool firstLook ) {
		if( _baselineVersion == _game.CheckpointVersion ) return;
		_baselineVersion = _game.CheckpointVersion;
		_baseline = _game.LatestCheckpoint;
		_picksSinceBaseline.Clear();
		// The first look after loading sees the very checkpoint that was loaded (a menu re-saves the state it was resumed at).
		moved |= !firstLook;
	}

	void Select( string optionText ) {
		string[] ids = OptionIds( _pending! );
		int index = Array.IndexOf( ids, optionText );
		if( index < 0 )
			throw new InvalidPickException( $"'{optionText}' is not an offered option for '{_pending!.Prompt}'." );
		IDecision decision = _pending!;
		_pending = null;
		_picksSinceBaseline.Add( optionText );
		_game.UserPortal.DecisionPortal.Choose( decision, decision.Options[index] );
	}

	ResultView? Result() => _game.EngineTask!.IsCompleted && _game.GameState.Result is { } r
		? new ResultView( r.Result.ToString(), r.Cause ) : null;

	/// <summary>
	/// Option text is the ID. The engine intends it to be unique but doesn't always achieve it, so:
	/// (1) Growth options are labelled with the group they belong to (the UI conveys this by layout), and
	/// (2) any remaining repeats get a ' [n]' suffix (n = occurrence, from 2).
	/// Selection maps back to the engine's option object by position.
	/// </summary>
	string[] OptionIds( IDecision decision ) {
		var seen = new Dictionary<string, int>();
		return [.. decision.Options.Select( o => {
			string text = decision is A.GrowthDecision or A.TargetedGrowthDecision ? GrowthLabel( o ) : o.Text;
			int n = seen[text] = seen.GetValueOrDefault( text ) + 1;
			return n == 1 ? text : $"{text} [{n}]";
		} )];
	}

	string GrowthLabel( IOption option ) {
		// a growth action together with the spirit it grows: label the action, keep what follows
		if( option is TargetedAction act )
			return $"{GrowthLabel( act.Action )} => {act.Target.Text}";
		GrowthGroup? group = _game.GameState.Spirits.Single().GrowthTrack.Groups
			.FirstOrDefault( g => g.GrowthActionFactories.Contains( option ) );
		return group is null || group.GrowthActionFactories.Length < 2 ? option.Text
			: $"{option.Text} (group: {group})";
	}

	readonly SoloGame _game;
	readonly bool _resumed;
	readonly List<(int Version, LogLineView Line)> _log = [];
	IDecision? _pending;
	Exception? _failure;
	JsonObject? _baseline;
	int _baselineVersion;
	readonly List<string> _picksSinceBaseline = [];
}
