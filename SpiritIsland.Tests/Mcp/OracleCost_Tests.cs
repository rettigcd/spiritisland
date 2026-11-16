#nullable enable
using System.Diagnostics;
using System.Text.Json.Nodes;
using SpiritIsland.Agent;
using Xunit.Abstractions;

namespace SpiritIsland.Tests.Mcp;

/// <summary>
/// Phase 0 of docs/WebClient-Plan.md: what does one stateless "oracle" request cost? Each step does exactly what a request
/// will: restore the game from its saved JSON (checkpoint plus the picks made since), apply one pick, settle on the next
/// decision and save again. Prints milliseconds by how many picks had to be replayed.
/// Games aren't safe to run concurrently, so everything runs serially from this one class.
///
/// Measured 2026-11-15 (Debug build, after a JIT warm-up; 274 requests over 4 spirits, 2 passive policies, board A, seed 1):
/// restoring the save takes a median 6 ms (p95 10, max 23); a whole request (restore, pick, settle, save) a median 7 ms
/// (p95 16, max 72); the save is a flat ~9 KB. Replaying up to 5 picks since the checkpoint showed no visible slowdown.
/// The 60-72 ms outliers are all the pick answering "Cascade blight from A4 to ..." (0 picks replayed); in every game the
/// passive play loses to a blighted island in round 4, so these are probably the request that ends the game.
/// Not covered: anything after round 4 (random and first-option play never survive that long), and actions with many
/// sub-decisions - a competent player's games would be a better sample.
/// </summary>
public class OracleCost_Tests( ITestOutputHelper output ) {

	[Fact]
	public void PrintCostPerRequest() {
		NewGameRequest[] games = [
			new( RiverSurges.Name, "A", 1 ),
			new( LightningsSwiftStrike.Name, "A", 1 ),
			new( Shadows.Name, "A", 1 ),
			new( ASpreadOfRampantGreen.Name, "A", 1, "England", 2 ),
		];

		PlayThroughRequests( games[0], rounds: 2 ); // warm-up: a server pays JIT once, not per request

		var all = new List<Sample>();
		foreach( bool firstOption in new[] { false, true } )
			foreach( NewGameRequest request in games ) {
				List<Sample> samples = PlayThroughRequests( request, rounds: 8, firstOption );
				output.WriteLine( $"== {request.Spirit} ({(firstOption ? "always first option" : "random picks")}): {samples.Count} requests, last round {samples[^1].Round}" );
				Report( samples );
				all.AddRange( samples );
			}

		output.WriteLine( "== All games, by picks replayed since the checkpoint" );
		Report( all );
		output.WriteLine( "== Slowest requests" );
		foreach( Sample s in all.OrderByDescending( s => s.RequestMs ).Take( 6 ) )
			output.WriteLine( $"{s.RequestMs,6:F0} ms  (restore {s.RestoreMs:F0} ms, {s.PicksReplayed} picks replayed)  {s.Prompt}" );
		all.ShouldNotBeEmpty();
	}

	record Sample( int PicksReplayed, double RestoreMs, double RequestMs, int SaveBytes, string Prompt, int Round );

	/// <summary> Plays random picks, one oracle request per pick, until the round limit or the end of the game. </summary>
	static List<Sample> PlayThroughRequests( NewGameRequest request, int rounds, bool firstOption = false ) {
		var samples = new List<Sample>();
		var rnd = new Random( 1 );
		string saveText;
		{
			var first = new AgentGame( request ) { AutoSelectSingleOption = false };
			saveText = first.Save().ToJsonString();
		}

		for( int step = 0; step < 4_000; ++step ) {
			JsonObject save = JsonNode.Parse( saveText )!.AsObject();
			int picksReplayed = save["Picks"]!.AsArray().Count;

			long start = Stopwatch.GetTimestamp();
			AgentGame game = AgentGame.Restore( save );
			double restoreMs = Stopwatch.GetElapsedTime( start ).TotalMilliseconds;

			DecisionSnapshot? decision = game.GetDecision();
			if( decision is null || game.GameState.RoundNumber > rounds ) break;
			game.Select( decision.Version, decision.Options[firstOption ? 0 : rnd.Next( decision.Options.Length )] );
			game.GetDecision();
			saveText = game.Save().ToJsonString();
			double requestMs = Stopwatch.GetElapsedTime( start ).TotalMilliseconds;

			samples.Add( new Sample( picksReplayed, restoreMs, requestMs, saveText.Length, decision.Prompt, game.GameState.RoundNumber ) );
		}
		return samples;
	}

	void Report( List<Sample> samples ) {
		output.WriteLine( "picks replayed |  n | restore ms (median / p95 / max) | full request ms (median / p95 / max) | save KB" );
		foreach( var group in samples.GroupBy( s => Bucket( s.PicksReplayed ) ).OrderBy( g => g.Key.Order ) )
			output.WriteLine( $"{group.Key.Label,14} | {group.Count(),3} | {Stats( group.Select( s => s.RestoreMs ) )} | {Stats( group.Select( s => s.RequestMs ) )} | {group.Average( s => s.SaveBytes ) / 1024:F0}" );
		output.WriteLine( $"{"all",14} | {samples.Count,3} | {Stats( samples.Select( s => s.RestoreMs ) )} | {Stats( samples.Select( s => s.RequestMs ) )} | {samples.Average( s => s.SaveBytes ) / 1024:F0}" );
	}

	static (int Order, string Label) Bucket( int picks ) => picks switch {
		0 => (0, "0"),
		<= 2 => (1, "1-2"),
		<= 5 => (2, "3-5"),
		<= 10 => (3, "6-10"),
		_ => (4, "11+"),
	};

	static string Stats( IEnumerable<double> values ) {
		double[] v = [.. values.Order()];
		double At( double p ) => v[Math.Min( v.Length - 1, (int)(p * v.Length) )];
		return $"{At( .5 ),6:F0} / {At( .95 ),6:F0} / {v[^1],6:F0}";
	}
}
