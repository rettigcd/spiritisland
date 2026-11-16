using System.Net;
using System.Net.Http.Json;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;
using Xunit.Abstractions;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// Plays every spirit with random picks and reports which ones the engine fails on. These are engine problems found while
/// building the web server, not oracle problems: all of them are about saving or restoring state, which the oracle does at
/// every action boundary, so it meets them sooner than the other UIs do. The test keeps this list honest: it fails when a
/// new spirit starts failing, and when a listed one stops, so the entry gets removed.
/// Deterministic: the same seeds and random picks every run.
/// </summary>
[Trait( "Speed", "Slow" )] // about 20 seconds: every spirit, a whole game each
public class EverySpirit_Tests( GamesFactory factory, ITestOutputHelper output ) : IClassFixture<GamesFactory> {

	/// <summary> Spirit name to the start of the engine's error message. </summary>
	static readonly Dictionary<string, string> KnownFailures = new() {
		// Can't be saved yet.
		["Finder Of Paths Unseen"] = "Spirit.ToJson doesn't support IActionFactory of type OpenTheWays",
		["Fractured Days Split the Sky"] = "SelfCmdRegistry doesn't know how to serialize IActOn<Spirit> of type RepeatableSelfCmd",
		["Stone's Unyielding Defiance"] = "Don't know how to serialize ISpaceEntity of type SpiritIsland.JaggedEarth.StubbornSolidityBehavior",
		["Sun Bright Whirlwind"] = "Don't know how to serialize ISpaceEntity of type SpiritIsland.AdjustHealth",
		["Towering Roots of the Jungle"] = "Sequence contains no elements",
		["Volcano Looming High"] = "The given key 'M' was not present in the dictionary",
		// Can't be restored: when a Still-Healthy blight card is replaced, the old card's start-of-Invader-Phase hook stays
		// in PreInvaderPhaseActions, and restoring resolves a hook named for a card to the *current* card.
		["Many Minds Move as One"] = "to type 'SpiritIsland.IRunBeforeInvaderPhase'",
		["Relentless Gaze of the Sun"] = "to type 'SpiritIsland.IRunBeforeInvaderPhase'",
		["Thunderspeaker"] = "to type 'SpiritIsland.IRunBeforeInvaderPhase'",
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

		foreach( var f in failed ) output.WriteLine( $"{f.Key}: {f.Value}" );

		string[] unexpected = [.. failed.Where( f => !KnownFailures.TryGetValue( f.Key, out string? expected ) || !f.Value.Contains( expected ) )
			.Select( f => $"{f.Key}: {f.Value}" )];
		string[] fixedNow = [.. KnownFailures.Keys.Where( spirit => !failed.ContainsKey( spirit ) )];

		unexpected.ShouldBeEmpty( "the engine failed on spirits, or with errors, not in KnownFailures" );
		fixedNow.ShouldBeEmpty( "these no longer fail - remove them from KnownFailures" );
	}
}
