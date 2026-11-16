using System.Net;
using System.Net.Http.Json;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// Plays every spirit with random picks and reports which ones the engine fails on. These are engine problems found while
/// building the web server, not oracle problems: the SI01 ones also happen in plain in-memory play, and the others are
/// state the engine can't save yet (the oracle saves a checkpoint at every action boundary, so it meets them sooner than
/// the other UIs do). The test keeps this list honest: it fails when a new spirit starts failing, and when a listed one
/// stops, so the entry gets removed.
/// Deterministic: the same seeds and random picks every run.
/// </summary>
[Trait( "Speed", "Slow" )] // about 20 seconds: every spirit, a whole game each
public class EverySpirit_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	/// <summary> Spirit name to the start of the engine's error message. </summary>
	static readonly Dictionary<string, string> KnownFailures = new() {
		// Spirit.ToJson / SelfCmdRegistry / Tokens can't save these yet.
		["Finder Of Paths Unseen"] = "Spirit.ToJson doesn't support IActionFactory of type OpenTheWays",
		["Fractured Days Split the Sky"] = "SelfCmdRegistry doesn't know how to serialize IActOn<Spirit> of type RepeatableSelfCmd",
		["Stone's Unyielding Defiance"] = "Don't know how to serialize ISpaceEntity of type SpiritIsland.JaggedEarth.StubbornSolidityBehavior",
		["Sun Bright Whirlwind"] = "Don't know how to serialize ISpaceEntity of type SpiritIsland.AdjustHealth",
		["Towering Roots of the Jungle"] = "Sequence contains no elements",
		["Volcano Looming High"] = "The given key 'M' was not present in the dictionary",
		// An ActionScope is disposed out of order during a Ravage; plain in-memory play does it too.
		["Many Minds Move as One"] = "Error SI01",
		["Ocean's Hungry Grasp"] = "Error SI01",
		["Relentless Gaze of the Sun"] = "Error SI01",
		["River Surges in Sunlight"] = "Error SI01",
		["Sharp Fangs Behind the Leaves"] = "Error SI01",
		["Thunderspeaker"] = "Error SI01",
	};

	[Fact]
	public async Task EverySpirit_PlaysUntilTheEngineFails_OnlyWhereItIsKnownTo() {
		HttpClient http = factory.CreateClient();
		var failed = new Dictionary<string, string>();
		int seed = 0;

		foreach( string spirit in GameSetup.Spirits ) {
			var rnd = new Random( ++seed );
			HttpResponseMessage created = await http.PostAsJsonAsync( "/api/games", new NewGameRequest( spirit, "A", seed ) );
			created.EnsureSuccessStatusCode();
			OracleResponse last = ( await created.Content.ReadFromJsonAsync<OracleResponse>() )!;

			for( int step = 0; step < 100 && last.Decision is not null; ++step ) {
				string pick = last.Decision.Options[rnd.Next( last.Decision.Options.Length )].Id;
				HttpResponseMessage response = await http.PostAsJsonAsync( $"/api/games/{last.GameId}/decide",
					new DecideRequest( last.CheckpointVersion, [.. last.Picks, pick] ) );
				if( response.StatusCode == HttpStatusCode.InternalServerError ) {
					failed[spirit] = ( await response.Content.ReadFromJsonAsync<ErrorView>() )!.Error;
					break;
				}
				response.EnsureSuccessStatusCode();
				last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
			}
		}

		string[] unexpected = [.. failed.Where( f => !KnownFailures.TryGetValue( f.Key, out string? expected ) || !f.Value.Contains( expected ) )
			.Select( f => $"{f.Key}: {f.Value}" )];
		string[] fixedNow = [.. KnownFailures.Keys.Where( spirit => !failed.ContainsKey( spirit ) )];

		unexpected.ShouldBeEmpty( "the engine failed on spirits, or with errors, not in KnownFailures" );
		fixedNow.ShouldBeEmpty( "these no longer fail - remove them from KnownFailures" );
	}
}
