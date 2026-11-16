using System.Collections.Concurrent;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Oracle;

/// <summary> What a decide call came to. The endpoint layer maps these to status codes. </summary>
public abstract record DecideOutcome {
	public sealed record Ok( OracleResponse Response ) : DecideOutcome;
	public sealed record NotFound : DecideOutcome;
	/// <summary> The client built on an older checkpoint. <see cref="Current"/> is where the game really is, with no picks pending. </summary>
	public sealed record Stale( OracleResponse Current ) : DecideOutcome;
	public sealed record BadPick( string Message ) : DecideOutcome;
}

/// <summary>
/// The stateless oracle (docs/WebClient-Plan.md): each call loads the game's checkpoint, replays the client's picks, and
/// reports the next decision. When the replay crosses an action boundary the new checkpoint is saved and the picks it
/// absorbed are dropped from the response.
/// </summary>
public sealed class OracleService( IGameFiles files ) {

	/// <exception cref="ArgumentException">the request names something that doesn't exist</exception>
	public async Task<OracleResponse> CreateAsync( NewGameRequest request ) {
		GameConfiguration config = GameSetup.Configure( request );
		SessionState state = await Task.Run( () => OracleSession.Start( config ) ); // own execution context: ActionScope is ambient
		var record = new GameRecord( files.NewId(), config, CheckpointVersion: 1, state.Checkpoint );
		files.Write( record );
		return Respond( record, state, state.Picks );
	}

	public async Task<DecideOutcome> DecideAsync( string id, DecideRequest request ) {
		SemaphoreSlim gate = _gates.GetOrAdd( id, _ => new SemaphoreSlim( 1, 1 ) ); // one request at a time per game
		await gate.WaitAsync();
		try {
			GameRecord? record = files.Read( id );
			if( record is null ) return new DecideOutcome.NotFound();

			if( request.CheckpointVersion != record.CheckpointVersion ) {
				SessionState current = await Task.Run( () => OracleSession.Resume( record, record.Over ? record.FinishingPicks! : [] ) );
				return new DecideOutcome.Stale( Respond( record, current, [] ) );
			}
			if( record.Over && request.Picks.Length > 0 )
				return new DecideOutcome.BadPick( "The game is over." );

			SessionState state;
			try {
				state = await Task.Run( () => OracleSession.Resume( record, record.Over ? record.FinishingPicks! : request.Picks ) );
			}
			catch( InvalidPickException ex ) {
				return new DecideOutcome.BadPick( ex.Message );
			}

			if( !record.Over && state.Result is not null ) {
				record = record with { CheckpointVersion = record.CheckpointVersion + 1, Checkpoint = state.Checkpoint, Over = true, FinishingPicks = state.Picks };
				files.Write( record );
			}
			else if( !record.Over && state.CheckpointMoved ) {
				record = record with { CheckpointVersion = record.CheckpointVersion + 1, Checkpoint = state.Checkpoint };
				files.Write( record );
			}
			return new DecideOutcome.Ok( Respond( record, state, record.Over ? [] : state.Picks ) );
		}
		finally {
			gate.Release();
		}
	}

	static OracleResponse Respond( GameRecord record, SessionState state, string[] picks )
		=> new( record.Id, record.CheckpointVersion, picks, state.View, state.Decision, state.Result );

	readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();
}
