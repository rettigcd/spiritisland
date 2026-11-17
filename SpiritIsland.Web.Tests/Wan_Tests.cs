using System.Net;
using Microsoft.Extensions.Logging;
using SpiritIsland.Web.Network;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// --wan: asking the router to forward the port, keeping it going and taking it away. These use a fake router and short intervals;
/// nothing here talks to a real router, so the UPnP calls themselves (MonoNatRouter) are not covered.
/// </summary>
public class Wan_Tests {

	#region fakes

	sealed class FakeRouter : IRouter {
		public readonly List<string> Calls = [];
		public IPAddress? External { get; set; } = IPAddress.Parse( "203.0.113.7" );
		/// <summary> Which MapPort calls (1-based) the router refuses. </summary>
		public HashSet<int> RefuseMapCalls { get; } = [];
		int _maps;

		public Task<IPAddress?> GetExternalAddressAsync() { lock( Calls ) Calls.Add( "external" ); return Task.FromResult( External ); }

		public Task MapPortAsync( int port, TimeSpan lease, string description ) {
			lock( Calls ) {
				++_maps;
				Calls.Add( $"map {port} {lease}" );
			}
			return RefuseMapCalls.Contains( _maps ) ? throw new InvalidOperationException( "refused" ) : Task.CompletedTask;
		}

		public Task UnmapPortAsync( int port ) { lock( Calls ) Calls.Add( $"unmap {port}" ); return Task.CompletedTask; }

		public string[] Snapshot() { lock( Calls ) return [.. Calls]; }
		public int Maps => Snapshot().Count( c => c.StartsWith( "map " ) );
	}

	sealed class FakeFinder( IRouter? router ) : IRouterFinder {
		public Task<IRouter?> FindAsync( TimeSpan timeout, CancellationToken cancel ) => Task.FromResult( router );
	}

	sealed class ListLogger : ILogger<WanPortForwarder> {
		public readonly List<(LogLevel Level, string Text)> Entries = [];
		public IDisposable? BeginScope<TState>( TState state ) where TState : notnull => null;
		public bool IsEnabled( LogLevel logLevel ) => true;
		public void Log<TState>( LogLevel level, EventId id, TState state, Exception? ex, Func<TState, Exception?, string> format ) {
			lock( Entries ) Entries.Add( (level, format( state, ex )) );
		}
		public string[] Texts( LogLevel level ) { lock( Entries ) return [.. Entries.Where( e => e.Level == level ).Select( e => e.Text )]; }
		public string All() { lock( Entries ) return string.Join( "\n", Entries.Select( e => e.Text ) ); }
	}

	static WanSettings Quick( bool startingToken = false, Func<string, CancellationToken, Task<bool>>? selfCheck = null ) => new(
		Lease: TimeSpan.FromHours( 1 ),
		RenewEvery: TimeSpan.FromMilliseconds( 30 ),
		DiscoveryTimeout: TimeSpan.FromSeconds( 1 ),
		RemoveTimeout: TimeSpan.FromSeconds( 1 ),
		startingToken,
		selfCheck ?? ( ( _, _ ) => Task.FromResult( true ) )
	);

	static async Task WaitUntil( Func<bool> condition, string what ) {
		for( int i = 0; i < 200 && !condition(); ++i ) await Task.Delay( 10 );
		condition().ShouldBeTrue( "timed out waiting for " + what );
	}

	#endregion fakes

	[Fact]
	public void TheRealSettings_LeaseForAnHour_AndRenewEveryHalfHour() {
		WanSettings settings = WanSettings.Normal( tokenIsStartingValue: false );

		settings.Lease.ShouldBe( TimeSpan.FromHours( 1 ) );
		settings.RenewEvery.ShouldBe( TimeSpan.FromMinutes( 30 ) );
		settings.RenewEvery.ShouldBeLessThan( settings.Lease ); // a renewal can be missed without losing the forwarding
	}

	[Fact]
	public async Task ForwardsThePort_WithTheLease() {
		var router = new FakeRouter();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), new ListLogger(), Quick() );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => router.Maps >= 1, "the first mapping" );
		await forwarder.StopAsync( CancellationToken.None );

		router.Snapshot().First().ShouldBe( "map 5000 01:00:00" );
	}

	[Fact]
	public async Task RenewsTheForwarding_WhileItRuns() {
		var router = new FakeRouter();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), new ListLogger(), Quick() );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => router.Maps >= 4, "several renewals" );
		await forwarder.StopAsync( CancellationToken.None );

		router.Snapshot().Where( c => c.StartsWith( "map " ) ).ShouldAllBe( c => c == "map 5000 01:00:00" );
	}

	[Fact]
	public async Task TakesTheForwardingAway_WhenTheServerStops_AndDoesNothingAfter() {
		var router = new FakeRouter();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), new ListLogger(), Quick() );
		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => router.Maps >= 2, "a renewal" );

		await forwarder.StopAsync( CancellationToken.None );

		string[] calls = router.Snapshot();
		calls.Last().ShouldBe( "unmap 5000" );
		await Task.Delay( 120 ); // longer than several renewal intervals
		router.Snapshot().Length.ShouldBe( calls.Length ); // nothing renews after it stopped
	}

	[Fact]
	public async Task WithNoRouter_LogsIt_ForwardsNothing_AndStopsCleanly() {
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( null ), log, Quick() );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.Texts( LogLevel.Warning ).Length > 0, "the warning" );
		await forwarder.StopAsync( CancellationToken.None );

		log.Texts( LogLevel.Warning ).Single().ShouldContain( "no UPnP router answered" );
	}

	[Fact]
	public async Task WhenTheRouterRefuses_LogsIt_DoesNotThrow_AndHasNothingToTakeAway() {
		var router = new FakeRouter { RefuseMapCalls = { 1 } };
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), log, Quick() );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.Texts( LogLevel.Warning ).Length > 0, "the warning" );
		await forwarder.StopAsync( CancellationToken.None );

		log.Texts( LogLevel.Warning ).First().ShouldContain( "would not forward port 5000" );
		router.Snapshot().ShouldNotContain( "unmap 5000" );
	}

	[Fact]
	public async Task AFailedRenewal_IsTriedAgainAtTheNextInterval() {
		var router = new FakeRouter { RefuseMapCalls = { 2 } }; // the first renewal fails
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), log, Quick() );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => router.Maps >= 4, "renewals after the failed one" );
		await forwarder.StopAsync( CancellationToken.None );

		log.Texts( LogLevel.Warning ).ShouldContain( t => t.Contains( "would not forward" ) );
		router.Snapshot().Last().ShouldBe( "unmap 5000" ); // it was still forwarded in the end, so it is taken away
	}

	[Fact]
	public async Task SaysTheAddressToUse_AndChecksItFromHere() {
		var router = new FakeRouter();
		var checkedUrls = new List<string>();
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5081, new FakeFinder( router ), log, Quick( selfCheck: ( url, _ ) => { checkedUrls.Add( url ); return Task.FromResult( true ); } ) );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.All().Contains( "reachable from outside" ), "the check" );
		await forwarder.StopAsync( CancellationToken.None );

		log.All().ShouldContain( "http://203.0.113.7:5081" );
		checkedUrls.ShouldBe( ["http://203.0.113.7:5081"] );
	}

	[Fact]
	public async Task WhenTheCheckFails_ItSaysThatCanBeNormal_NotThatItIsBroken() {
		var router = new FakeRouter();
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), log, Quick( selfCheck: ( _, _ ) => Task.FromResult( false ) ) );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.All().Contains( "could not reach" ), "the check" );
		await forwarder.StopAsync( CancellationToken.None );

		log.All().ShouldContain( "normal for some routers" );
		log.Texts( LogLevel.Error ).ShouldBeEmpty();
	}

	[Fact]
	public async Task AnOutsideAddressThatIsPrivate_IsWarnedAbout_AndNotChecked()
	{
		var router = new FakeRouter { External = IPAddress.Parse( "192.168.1.1" ) };
		var log = new ListLogger();
		bool checkedIt = false;
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( router ), log, Quick( selfCheck: ( _, _ ) => { checkedIt = true; return Task.FromResult( true ); } ) );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.Texts( LogLevel.Warning ).Any( t => t.Contains( "not an address on the internet" ) ), "the warning" );
		await forwarder.StopAsync( CancellationToken.None );

		checkedIt.ShouldBeFalse();
	}

	[Theory]
	[InlineData( true )]
	[InlineData( false )]
	public async Task WarnsWhenTheAccessTokenIsStillTheStartingOne( bool startingToken ) {
		var log = new ListLogger();
		using var forwarder = new WanPortForwarder( 5000, new FakeFinder( new FakeRouter() ), log, Quick( startingToken ) );

		await forwarder.StartAsync( CancellationToken.None );
		await WaitUntil( () => log.Texts( LogLevel.Warning ).Length > 0, "a warning" );
		await forwarder.StopAsync( CancellationToken.None );

		log.Texts( LogLevel.Warning ).Any( t => t.Contains( "still its starting value" ) ).ShouldBe( startingToken );
		log.Texts( LogLevel.Warning ).ShouldNotBeEmpty(); // either way it says only the shared token protects it
	}

	[Theory]
	[InlineData( "10.1.2.3", true )]
	[InlineData( "172.16.0.1", true )]
	[InlineData( "172.31.255.255", true )]
	[InlineData( "172.32.0.1", false )]
	[InlineData( "192.168.0.1", true )]
	[InlineData( "100.64.0.1", true )]  // carrier-grade NAT
	[InlineData( "100.128.0.1", false )]
	[InlineData( "169.254.1.1", true )]
	[InlineData( "127.0.0.1", true )]
	[InlineData( "203.0.113.7", false )]
	[InlineData( "8.8.8.8", false )]
	public void WhichAddressesAreNotOnTheInternet( string address, bool notPublic )
		=> WanPortForwarder.IsNotPublic( IPAddress.Parse( address ) ).ShouldBe( notPublic );

	#region command line

	[Fact]
	public void Options_AreTakenOutOfTheArguments_SoTheyDoNotSwallowTheNextOne() {
		ServerOptions options = ServerOptions.FromArgs( ["--urls", "http://localhost:5081", "--wan", "--lan"] );

		options.Lan.ShouldBeTrue();
		options.Wan.ShouldBeTrue();
		options.Rest.ShouldBe( ["--urls", "http://localhost:5081"] );
	}

	[Fact]
	public void NoOptions_ListensOnlyOnThisComputer() {
		ServerOptions options = ServerOptions.FromArgs( ["--urls", "http://localhost:5081"] );

		options.ListenOnNetwork.ShouldBeFalse();
		options.Wan.ShouldBeFalse();
	}

	[Theory]
	[InlineData( "--lan", true, false )]
	[InlineData( "--wan", true, true )] // --wan needs the whole network too
	public void ListensOnTheNetwork_ForLanAndForWan( string option, bool listensOnNetwork, bool wan ) {
		ServerOptions options = ServerOptions.FromArgs( [option] );

		options.ListenOnNetwork.ShouldBe( listensOnNetwork );
		options.Wan.ShouldBe( wan );
	}

	[Theory]
	[InlineData( null, 5000 )]
	[InlineData( "http://localhost:5081", 5081 )]
	[InlineData( "http://*:6000;https://*:6001", 6000 )]
	[InlineData( "http://0.0.0.0:7000", 7000 )]
	[InlineData( "http://+:8000", 8000 )]
	public void ThePortThatWouldHaveBeenUsed( string? urls, int port ) => ServerOptions.PortOf( urls ).ShouldBe( port );

	#endregion command line
}
