using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SpiritIsland.Basegame;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary> Hosts the app with its games in a throwaway directory. </summary>
public sealed class GamesFactory : WebApplicationFactory<Program> {
	public string GamesDir { get; } = Path.Combine( Path.GetTempPath(), "spiritisland-web-tests-" + Guid.NewGuid().ToString( "N" ) );

	protected override void ConfigureWebHost( IWebHostBuilder builder ) => builder.UseSetting( "GamesDir", GamesDir );

	protected override void Dispose( bool disposing ) {
		base.Dispose( disposing );
		if( disposing && Directory.Exists( GamesDir ) ) Directory.Delete( GamesDir, recursive: true );
	}
}

/// <summary>
/// Phase 1 of docs/WebClient-Plan.md: the oracle played through HTTP, with no UI. Games are only known to be safe to run
/// concurrently in separate execution contexts, which is what the server gives each request, so the one test that plays
/// two games at once is deliberate and the rest run one at a time.
/// </summary>
public class OracleEndpoint_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	static readonly NewGameRequest River = new( RiverSurges.Name, "A", Seed: 1 );

	#region setup and creating

	[Fact]
	public async Task Options_ListSpiritsBoardsAndAdversaries() {
		var options = await factory.CreateClient().GetFromJsonAsync<SetupOptions>( "/api/options" );

		options!.Spirits.ShouldContain( RiverSurges.Name );
		options.Boards.ShouldContain( "A" );
		options.Adversaries.ShouldNotBeEmpty();
	}

	[Fact]
	public async Task NewGame_ReturnsTheFirstDecision() {
		var player = new Player( factory );
		OracleResponse first = await player.NewAsync( River );

		first.CheckpointVersion.ShouldBe( 1 );
		first.Picks.ShouldBeEmpty();
		first.Result.ShouldBeNull();
		first.Decision.ShouldNotBeNull();
		first.Decision.Options.ShouldNotBeEmpty();
		first.Decision.Options.Select( o => o.Id ).ShouldBeUnique();
	}

	[Fact]
	public async Task NewGame_WithUnknownSpirit_Is400() {
		HttpResponseMessage response = await factory.CreateClient().PostAsJsonAsync( "/api/games", new NewGameRequest( "No Such Spirit", "A" ) );

		response.StatusCode.ShouldBe( HttpStatusCode.BadRequest );
		( await response.Content.ReadFromJsonAsync<ErrorView>() )!.Error.ShouldContain( "Unknown spirit" );
	}

	[Fact]
	public async Task NewGame_WithAnAdversary_Starts() {
		OracleResponse first = await new Player( factory ).NewAsync( new( RiverSurges.Name, "A", 2, "England", 1 ) );

		first.Decision.ShouldNotBeNull();
	}

	[Fact]
	public async Task Games_ListsTheGamesThatWereCreated() {
		var player = new Player( factory );
		OracleResponse first = await player.NewAsync( River );

		var games = await factory.CreateClient().GetFromJsonAsync<GameSummary[]>( "/api/games" );

		games!.ShouldContain( g => g.GameId == first.GameId && g.Spirit == RiverSurges.Name && !g.Over );
	}

	#endregion setup and creating

	#region playing

	[Fact]
	public async Task TakingTheFirstOptionEachTime_PlaysTheGameToTheEnd() {
		var player = new Player( factory );
		await player.NewAsync( River );

		int mostPending = 0;
		for( int step = 0; step < 2_000 && player.Last.Decision is not null; ++step ) {
			await player.PickAsync( player.Last.Decision.Options[0].Id );
			mostPending = Math.Max( mostPending, player.Last.Picks.Length );
			OracleSession.EnginesRunning.ShouldBe( 0, $"an engine task was left running after step {step}" );
		}

		player.Last.Decision.ShouldBeNull( "the game should have ended" );
		player.Last.Result.ShouldNotBeNull();
		player.Last.Result.Outcome.ShouldBeOneOf( "Victory", "Defeat" );
		// finished actions are dropped from the client's list, so it doesn't grow with the length of the game
		mostPending.ShouldBeLessThan( 25 );
	}

	[Fact]
	public async Task NoEngineIsLeftRunning_AfterAnyKindOfRequest() {
		var player = new Player( factory );
		OracleResponse first = await player.NewAsync( River );
		OracleSession.EnginesRunning.ShouldBe( 0, "after creating a game" );

		for( int i = 0; i < 8; ++i ) { // mid-action as well as at boundaries
			await player.PickAsync( player.Last.Decision!.Options[^1].Id );
			OracleSession.EnginesRunning.ShouldBe( 0, $"after pick {i}" );
		}
		await player.ReadAsync();
		OracleSession.EnginesRunning.ShouldBe( 0, "after reading" );
		await player.DecideRawAsync( player.Last.CheckpointVersion, ["not an option"] );
		OracleSession.EnginesRunning.ShouldBe( 0, "after a rejected pick" );
		await player.DecideRawAsync( 0, [] );
		OracleSession.EnginesRunning.ShouldBe( 0, "after a stale request" );
	}

	[Fact]
	public async Task RepeatingARequest_GivesTheSameAnswer() {
		var player = new Player( factory );
		await player.NewAsync( River );
		for( int i = 0; i < 6; ++i )
			await player.PickAsync( player.Last.Decision!.Options[0].Id );

		OracleResponse again = await player.ReadAsync();
		OracleResponse andAgain = await player.ReadAsync();

		again.ShouldBe( player.Last, new ResponseComparer() );
		andAgain.ShouldBe( player.Last, new ResponseComparer() );
	}

	[Fact]
	public async Task PicksMadeOneAtATime_GiveTheSameGameAsPicksSentTogether() {
		// One client sends each pick as it makes it; the other sends them all in a single request.
		var stepwise = new Player( factory );
		await stepwise.NewAsync( River );
		var all = new List<string>();
		for( int i = 0; i < 12 && stepwise.Last.Decision is not null; ++i ) {
			string pick = stepwise.Last.Decision.Options[^1].Id; // the last option, so this isn't the first-option test's game
			await stepwise.PickAsync( pick );
			all.Add( pick );
		}

		// Picks that finished an action are absorbed into a checkpoint the second game doesn't have, so it starts from
		// its own first checkpoint and sends everything.
		var together = new Player( factory );
		await together.NewAsync( River );
		HttpResponseMessage response = await together.DecideRawAsync( 1, [.. all] );
		response.StatusCode.ShouldBe( HttpStatusCode.OK );
		var result = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;

		result.Decision.ShouldBe( stepwise.Last.Decision, new DecisionComparer() );
		Json( result.View ).ShouldBe( Json( stepwise.Last.View ) );
		result.Log.ShouldBe( stepwise.Last.Log ); // the log is the same whichever way the picks arrive
	}

	[Fact]
	public async Task GamesAreKeptOnDisk_SoAFreshHostContinuesThem() {
		using var first = new GamesFactory();
		var before = new Player( first );
		await before.NewAsync( River );
		for( int i = 0; i < 5; ++i )
			await before.PickAsync( before.Last.Decision!.Options[0].Id );

		using WebApplicationFactory<Program> second = first.WithWebHostBuilder( _ => { } ); // same directory, new host
		var after = new Player( second ) { Last = before.Last };
		OracleResponse continued = await after.ReadAsync();

		continued.ShouldBe( before.Last, new ResponseComparer() );
		await after.PickAsync( continued.Decision!.Options[0].Id ); // and it can go on from there
	}

	[Fact]
	public async Task TwoGamesPlayedAtTheSameTime_BothFinish() {
		static async Task<OracleResponse> PlayAsync( GamesFactory factory, NewGameRequest request ) {
			var player = new Player( factory );
			await player.NewAsync( request );
			for( int step = 0; step < 2_000 && player.Last.Decision is not null; ++step )
				await player.PickAsync( player.Last.Decision.Options[0].Id );
			return player.Last;
		}

		OracleResponse[] results = await Task.WhenAll(
			PlayAsync( factory, River ),
			PlayAsync( factory, new( RiverSurges.Name, "A", 3 ) ) );

		results.ShouldAllBe( r => r.Decision == null && r.Result != null );
		results[0].GameId.ShouldNotBe( results[1].GameId );
	}

	#endregion playing

	#region rejecting

	[Fact]
	public async Task StaleCheckpoint_Is409_WithTheCurrentState() {
		var player = new Player( factory );
		await player.NewAsync( River );
		while( player.Last.CheckpointVersion == 1 ) // play until an action finishes and the checkpoint moves
			await player.PickAsync( player.Last.Decision!.Options[0].Id );
		OracleResponse current = player.Last;

		HttpResponseMessage response = await player.DecideRawAsync( 1, [current.Decision!.Options[0].Id] );

		response.StatusCode.ShouldBe( HttpStatusCode.Conflict );
		var body = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		body.CheckpointVersion.ShouldBe( current.CheckpointVersion );
		body.Picks.ShouldBeEmpty();
		body.Decision.ShouldNotBeNull();
	}

	[Fact]
	public async Task PickThatIsNotOffered_Is400_AndChangesNothing() {
		var player = new Player( factory );
		OracleResponse first = await player.NewAsync( River );

		HttpResponseMessage response = await player.DecideRawAsync( first.CheckpointVersion, ["not an option"] );

		response.StatusCode.ShouldBe( HttpStatusCode.BadRequest );
		( await response.Content.ReadFromJsonAsync<ErrorView>() )!.Error.ShouldContain( "is not an offered option" );
		( await player.ReadAsync() ).ShouldBe( first, new ResponseComparer() );
	}

	[Fact]
	public async Task UnknownGame_Is404() {
		HttpResponseMessage response = await factory.CreateClient().PostAsJsonAsync( "/api/games/nosuchgame/decide", new DecideRequest( 1, [] ) );

		response.StatusCode.ShouldBe( HttpStatusCode.NotFound );
	}

	[Fact]
	public async Task GameIdThatIsNotAnId_Is404() {
		HttpResponseMessage response = await factory.CreateClient().PostAsJsonAsync( "/api/games/..%2F..%2Fsecret/decide", new DecideRequest( 1, [] ) );

		response.StatusCode.ShouldBe( HttpStatusCode.NotFound );
	}

	[Fact]
	public async Task PickAfterTheGameIsOver_Is400_ButItCanStillBeRead() {
		var player = new Player( factory );
		await player.NewAsync( River );
		for( int step = 0; step < 2_000 && player.Last.Decision is not null; ++step )
			await player.PickAsync( player.Last.Decision.Options[0].Id );
		OracleResponse over = player.Last;
		over.Result.ShouldNotBeNull();

		HttpResponseMessage response = await player.DecideRawAsync( over.CheckpointVersion, ["anything"] );
		OracleResponse read = await player.ReadAsync();

		response.StatusCode.ShouldBe( HttpStatusCode.BadRequest );
		read.ShouldBe( over, new ResponseComparer() );
	}

	#endregion rejecting

	#region the log and prompt details

	[Fact]
	public async Task NewGame_StartsTheLog() {
		OracleResponse first = await new Player( factory ).NewAsync( River );

		first.Log.ShouldContain( l => l.Kind == "phase" && l.Text.Contains( "Round 1" ) );
	}

	[Fact]
	public async Task TheLog_KeepsEverythingThatHappened_WhenTheGameMovesOn() {
		var player = new Player( factory );
		OracleResponse before = await player.NewAsync( River );
		// play until the log has grown and the checkpoint has moved on: what happened before is now before the checkpoint
		for( int i = 0; i < 80 && ( player.Last.Log.Length <= before.Log.Length || player.Last.CheckpointVersion == before.CheckpointVersion ); ++i )
			await player.PickAsync( player.Last.Decision!.Options[0].Id );

		player.Last.CheckpointVersion.ShouldBeGreaterThan( before.CheckpointVersion );
		player.Last.Log.Length.ShouldBeGreaterThan( before.Log.Length );
		player.Last.Log.Take( before.Log.Length ).ShouldBe( before.Log );

		// and asking again later - the picks since the checkpoint are replayed - neither loses nor repeats any of it
		( await player.ReadAsync() ).Log.ShouldBe( player.Last.Log );
	}

	[Fact]
	public async Task TheLog_IsKeptOnDisk_SoAFreshHostHasIt() {
		var player = new Player( factory );
		await player.NewAsync( River );
		while( player.Last.CheckpointVersion < 3 )
			await player.PickAsync( player.Last.Decision!.Options[0].Id );
		OracleResponse last = player.Last;

		using WebApplicationFactory<Program> fresh = factory.WithWebHostBuilder( _ => { } ); // same directory, new host
		var other = new Player( fresh ) { Last = last };
		( await other.ReadAsync() ).Log.ShouldBe( last.Log );
	}

	[Fact]
	public async Task StaleCheckpoint_ResponseCarriesTheLog() {
		var player = new Player( factory );
		await player.NewAsync( River );
		while( player.Last.CheckpointVersion == 1 )
			await player.PickAsync( player.Last.Decision!.Options[0].Id );
		OracleResponse current = player.Last;

		HttpResponseMessage response = await player.DecideRawAsync( 1, [] );

		response.StatusCode.ShouldBe( HttpStatusCode.Conflict );
		( await response.Content.ReadFromJsonAsync<OracleResponse>() )!.Log.ShouldBe( current.Log );
	}

	[Fact]
	public async Task PlayPowerCardPrompt_HasItsNumbersSpelledOut() {
		var player = new Player( factory );
		await player.NewAsync( River );
		for( int i = 0; i < 40 && !player.Last.Decision!.Prompt.StartsWith( "Play power card" ); ++i )
			await player.PickAsync( player.Last.Decision.Options[0].Id );

		player.Last.Decision!.Prompt.ShouldStartWith( "Play power card" );
		player.Last.Decision.Details.ShouldNotBeNull().ShouldMatch( @"^Energy: \d+\. Card plays left: \d+\.$" );
	}

	#endregion the log and prompt details

	/// <summary> What a browser holds: the last response, whose pending picks it adds to. </summary>
	sealed class Player( WebApplicationFactory<Program> host ) {
		readonly HttpClient _http = host.CreateClient();
		public OracleResponse Last { get; set; } = null!;

		public async Task<OracleResponse> NewAsync( NewGameRequest request ) {
			HttpResponseMessage response = await _http.PostAsJsonAsync( "/api/games", request );
			response.StatusCode.ShouldBe( HttpStatusCode.OK, await response.Content.ReadAsStringAsync() );
			return Last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}

		public Task<HttpResponseMessage> DecideRawAsync( int checkpointVersion, string[] picks )
			=> _http.PostAsJsonAsync( $"/api/games/{Last.GameId}/decide", new DecideRequest( checkpointVersion, picks ) );

		/// <summary> Makes one more pick on top of the ones already pending. </summary>
		public async Task<OracleResponse> PickAsync( string option ) {
			HttpResponseMessage response = await DecideRawAsync( Last.CheckpointVersion, [.. Last.Picks, option] );
			response.StatusCode.ShouldBe( HttpStatusCode.OK, await response.Content.ReadAsStringAsync() );
			return Last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}

		/// <summary> Asks again with the picks already pending, adding none. Doesn't change <see cref="Last"/>. </summary>
		public async Task<OracleResponse> ReadAsync() {
			HttpResponseMessage response = await DecideRawAsync( Last.CheckpointVersion, Last.Picks );
			response.StatusCode.ShouldBe( HttpStatusCode.OK, await response.Content.ReadAsStringAsync() );
			return ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}
	}

	// Records holding arrays don't compare by value, so compare the parts that matter.
	static string Json<T>( T value ) => System.Text.Json.JsonSerializer.Serialize( value );

	sealed class DecisionComparer : IEqualityComparer<DecisionView?> {
		public bool Equals( DecisionView? a, DecisionView? b )
			=> a is null ? b is null : b is not null && a.Prompt == b.Prompt && a.Options.SequenceEqual( b.Options );
		public int GetHashCode( DecisionView? d ) => d?.Prompt.GetHashCode() ?? 0;
	}

	sealed class ResponseComparer : IEqualityComparer<OracleResponse> {
		public bool Equals( OracleResponse? a, OracleResponse? b )
			=> a is null ? b is null : b is not null
				&& a.GameId == b.GameId && a.CheckpointVersion == b.CheckpointVersion && a.Picks.SequenceEqual( b.Picks )
				&& Json( a.View ) == Json( b.View ) && new DecisionComparer().Equals( a.Decision, b.Decision ) && a.Result == b.Result
				&& a.Log.SequenceEqual( b.Log );
		public int GetHashCode( OracleResponse r ) => HashCode.Combine( r.GameId, r.CheckpointVersion );
	}
}
