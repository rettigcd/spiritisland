using System.Net;

namespace SpiritIsland.Web.Network;

/// <summary> Finds the router on the local network that can forward ports (UPnP Internet Gateway Device). </summary>
public interface IRouterFinder {
	/// <returns>The router, or null if none answered within <paramref name="timeout"/>.</returns>
	Task<IRouter?> FindAsync( TimeSpan timeout, CancellationToken cancel );
}

/// <summary> What the server asks of the router. </summary>
public interface IRouter {
	/// <summary> The router's address on the internet side, if it says. </summary>
	Task<IPAddress?> GetExternalAddressAsync();

	/// <summary> Forwards TCP <paramref name="port"/> (the same port outside and in) to this computer for <paramref name="lease"/>; calling it again renews the lease. </summary>
	Task MapPortAsync( int port, TimeSpan lease, string description );

	Task UnmapPortAsync( int port );
}

/// <param name="Lease">How long the router keeps the forwarding if it is not renewed.</param>
/// <param name="RenewEvery">How often it is renewed: well inside the lease, so one missed renewal does not lose it.</param>
public record WanSettings(
	TimeSpan Lease,
	TimeSpan RenewEvery,
	TimeSpan DiscoveryTimeout,
	TimeSpan RemoveTimeout,
	bool TokenIsStartingValue,
	Func<string, CancellationToken, Task<bool>> SelfCheck
) {

	public static WanSettings Normal( bool tokenIsStartingValue ) => new(
		Lease: TimeSpan.FromHours( 1 ),
		RenewEvery: TimeSpan.FromMinutes( 30 ),
		DiscoveryTimeout: TimeSpan.FromSeconds( 15 ),
		RemoveTimeout: TimeSpan.FromSeconds( 10 ),
		tokenIsStartingValue,
		AnythingAnswersAsync
	);

	/// <summary> True if the address answers at all (even with a 401, which the access token gives a caller without it). </summary>
	static async Task<bool> AnythingAnswersAsync( string baseUrl, CancellationToken cancel ) {
		using var http = new HttpClient { Timeout = TimeSpan.FromSeconds( 5 ) };
		try {
			using HttpResponseMessage _ = await http.GetAsync( baseUrl + "/api/options", cancel );
			return true;
		}
		catch( Exception ) when( !cancel.IsCancellationRequested ) {
			return false;
		}
	}
}

/// <summary>
/// With --wan: asks the router to forward the server's port from the internet, keeps that going while the server runs, and takes it away
/// when the server stops.
///  - The forwarding is leased for an hour and renewed every 30 minutes, so if the server is killed (and cannot take it away) the router
///    drops it by itself within the hour.
///  - If no router answers, or it refuses, that is logged and the server carries on as a normal one (reachable on the local network).
///  - Once forwarding is set up it says the address to use, and tries it from here as a check. That check can fail while forwarding works,
///    because many routers do not send a request from inside the network to their own outside address back in.
/// There is no login beyond the shared access token and the connection is plain HTTP, so it warns when the token is still the starting one.
/// </summary>
public sealed class WanPortForwarder( int port, IRouterFinder finder, ILogger<WanPortForwarder> log, WanSettings settings ) : BackgroundService {

	protected override async Task ExecuteAsync( CancellationToken stopping ) {
		try {
			_router = await finder.FindAsync( settings.DiscoveryTimeout, stopping );
			if( _router is null ) {
				log.LogWarning( "--wan: no UPnP router answered, so nothing was forwarded. The server is still available on the local network." );
				return;
			}
			if( !await MapAsync() ) return;
			await ReportAsync( stopping );

			using var timer = new PeriodicTimer( settings.RenewEvery );
			while( await timer.WaitForNextTickAsync( stopping ) )
				await MapAsync(); // adding it again renews the lease
		}
		catch( OperationCanceledException ) when( stopping.IsCancellationRequested ) {
			// the server is stopping
		}
		catch( Exception ex ) {
			log.LogError( ex, "--wan: forwarding stopped because of an error." );
		}
	}

	/// <summary> Takes the forwarding away. </summary>
	public override async Task StopAsync( CancellationToken cancel ) {
		await base.StopAsync( cancel );
		if( _router is null || !_mapped ) return;
		try {
			using var limit = new CancellationTokenSource( settings.RemoveTimeout );
			await _router.UnmapPortAsync( port ).WaitAsync( limit.Token );
			_mapped = false;
			log.LogInformation( "--wan: stopped forwarding port {Port}.", port );
		}
		catch( Exception ex ) {
			log.LogWarning( ex, "--wan: could not stop forwarding port {Port}. The router drops it by itself when its lease ({Lease}) runs out.", port, settings.Lease );
		}
	}

	async Task<bool> MapAsync() {
		try {
			await _router!.MapPortAsync( port, settings.Lease, "Spirit Island" );
			_mapped = true;
			return true;
		}
		catch( Exception ex ) {
			log.LogWarning( ex, "--wan: the router would not forward port {Port}. It may already be forwarded to another computer, or the router may not allow it.", port );
			return false;
		}
	}

	async Task ReportAsync( CancellationToken stopping ) {
		IPAddress? external = null;
		try { external = await _router!.GetExternalAddressAsync(); }
		catch( Exception ex ) { log.LogWarning( ex, "--wan: the router did not say its outside address." ); }

		if( external is null ) {
			log.LogInformation( "--wan: port {Port} is forwarded. Use this network's outside address with that port.", port );
		}
		else {
			string url = $"http://{external}:{port}";
			log.LogInformation( "--wan: port {Port} is forwarded. From the internet, open {Url}", port, url );
			if( IsNotPublic( external ) )
				log.LogWarning( "--wan: {Address} is not an address on the internet. This router is probably behind another router or a carrier's shared address, so the server will not be reachable from outside.", external );
			else
				await CheckAsync( url, stopping );
		}

		if( settings.TokenIsStartingValue )
			log.LogWarning( "--wan: the access token is still its starting value. Anyone on the internet can try it; the connection is plain HTTP, so it can also be read in transit. Change AccessToken.Value." );
		else
			log.LogWarning( "--wan: this server is reachable from the internet. The only protection is the shared access token, over plain HTTP." );
	}

	async Task CheckAsync( string url, CancellationToken stopping ) {
		if( await settings.SelfCheck( url, stopping ) )
			log.LogInformation( "--wan: reached {Url} from here, so it is reachable from outside.", url );
		else
			log.LogInformation( "--wan: could not reach {Url} from here. That is normal for some routers (they do not send a request from inside back in), so try it from a phone with Wi-Fi off.", url );
	}

	/// <summary> Private, link-local, loopback and carrier-grade-NAT (100.64.0.0/10) addresses: none of them is reachable from the internet. </summary>
	public static bool IsNotPublic( IPAddress address ) {
		if( IPAddress.IsLoopback( address ) ) return true;
		byte[] b = address.GetAddressBytes();
		if( b.Length != 4 ) return false; // an IPv6 address: not judged
		return b[0] == 10
			|| b[0] == 172 && b[1] is >= 16 and <= 31
			|| b[0] == 192 && b[1] == 168
			|| b[0] == 169 && b[1] == 254
			|| b[0] == 100 && b[1] is >= 64 and <= 127
			|| b[0] == 0;
	}

	IRouter? _router;
	bool _mapped;
}
