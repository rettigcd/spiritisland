using System.Text.Json.Nodes;
using SpiritIsland.SinglePlayer;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Oracle;

/// <summary> A pick that isn't one of the offered options, or that comes after the game has ended. </summary>
public sealed class InvalidPickException( string message ) : Exception( message ) { }

/// <summary> What a session reached after replaying picks. </summary>
/// <param name="Checkpoint">The newest action-boundary checkpoint.</param>
/// <param name="CheckpointMoved">True if replaying crossed an action boundary, so <paramref name="Checkpoint"/> is newer than the one started from.</param>
/// <param name="Picks">The picks made since <paramref name="Checkpoint"/>.</param>
public record SessionState( JsonObject Checkpoint, bool CheckpointMoved, string[] Picks, GameView View, DecisionView? Decision, ResultView? Result );

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

	/// <summary> Engines started and not yet finished. Zero between requests; a count that keeps growing means cancelling isn't working. </summary>
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
		_baseline = startedAt;
		_game.Start();
		Interlocked.Increment( ref _enginesRunning );
		_game.EngineTask!.ContinueWith( _ => Interlocked.Decrement( ref _enginesRunning ), TaskScheduler.Default );
	}

	public void Dispose() => Cancel();

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
		foreach( string pick in picks ) {
			if( decision is null ) throw new InvalidPickException( $"Cannot pick '{pick}': the game is over." );
			Select( pick );
			decision = Settle( ref moved );
		}
		return new SessionState( _baseline!, moved, [.. _picksSinceBaseline], new GameView( _game.GameState.RoundNumber, _game.GameState.Phase.ToString() ),
			decision, Result() );
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
				return new DecisionView( _pending.Prompt, [.. OptionIds( _pending ).Zip( _pending.Options, ( id, o ) => new OptionView( id, o.Text ) )] );
			if( _game.EngineTask!.IsCompleted ) {
				SyncBaseline( ref moved, firstLook );
				return null;
			}
			if( Environment.TickCount64 > giveUpAt )
				throw new TimeoutException( "The game engine did not reach a decision within 30 seconds." );
		}
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
			string text = decision is A.GrowthDecision ? GrowthLabel( o ) : o.Text;
			int n = seen[text] = seen.GetValueOrDefault( text ) + 1;
			return n == 1 ? text : $"{text} [{n}]";
		} )];
	}

	string GrowthLabel( IOption option ) {
		GrowthGroup? group = _game.GameState.Spirits.Single().GrowthTrack.Groups
			.FirstOrDefault( g => g.GrowthActionFactories.Contains( option ) );
		return group is null || group.GrowthActionFactories.Length < 2 ? option.Text
			: $"{option.Text} (group: {group})";
	}

	readonly SoloGame _game;
	IDecision? _pending;
	JsonObject? _baseline;
	int _baselineVersion;
	readonly List<string> _picksSinceBaseline = [];
}
