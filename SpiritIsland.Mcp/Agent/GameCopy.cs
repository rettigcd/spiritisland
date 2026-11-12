using System.Text.Json.Nodes;
using SpiritIsland.Serialization;

namespace SpiritIsland.Agent;

/// <summary>
/// Runs work on a copy of the live game, so previews (predictions, target lists) can use the engine's own rules without
/// touching the real game. The copy is the live state serialized and restored onto a freshly built game, the same
/// mechanism rewind uses. Any decision the work meets is answered with the first option and recorded.
/// </summary>
static class GameCopy {

	public sealed class Result {
		public GameState? Copy;
		public List<string> Choices = [];
		public Exception? Error; // thrown by the work (e.g. GameOverException), unwrapped
		public bool TimedOut;
	}

	/// <param name="work">Runs on the copy, inside the copy's own ActionScope.</param>
	public static Result Run( AgentGame game, Func<GameState, Task> work, int maxSeconds = 30 ) {
		GameState live = game.GameState;
		ActionScope.Initialize( live.RootScope );
		JsonObject snapshot = live.ToJson( new GameStateSerializationContext( live ) );

		var result = new Result();
		var setUp = new TaskCompletionSource<GameState>();
		Task task = Task.Run( async () => {
			GameState copy = GameSetup.Build( game.Request );
			ActionScope.Initialize( copy.RootScope );
			copy.RestoreFromJson( snapshot, new GameStateSerializationContext( copy ) );
			result.Copy = copy;
			setUp.SetResult( copy );
			await work( copy );
		} );

		// Answer the decisions the work meets, until it finishes.
		var deadline = DateTime.UtcNow.AddSeconds( maxSeconds );
		Task.WaitAny( [setUp.Task, task], TimeSpan.FromSeconds( maxSeconds ) ); // set-up done, or it failed
		while( setUp.Task.IsCompletedSuccessfully && !task.IsCompleted ) {
			IDecisionPortal portal = setUp.Task.Result.Spirits.Single().Portal;
			if( portal.WaitForNext( 20 ) && portal.Next is { } d ) {
				IOption choice = d.Options[0];
				result.Choices.Add( $"'{d.Prompt}': chose '{choice.Text}' (first of {d.Options.Length})" );
				portal.Choose( d, choice );
			}
			if( DateTime.UtcNow > deadline || result.Choices.Count > 200 ) {
				result.TimedOut = true; // the abandoned copy is left waiting; it holds no reference to the live game
				break;
			}
		}

		try {
			if( !result.TimedOut && !task.Wait( TimeSpan.FromSeconds( 5 ) ) ) result.TimedOut = true;
		} catch( AggregateException ex ) {
			result.Error = ex.InnerException;
		}
		if( result.Copy is not null )
			ActionScope.Initialize( result.Copy.RootScope ); // so the caller can read the copy's final state
		return result;
	}
}
