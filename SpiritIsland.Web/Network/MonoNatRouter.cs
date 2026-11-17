using System.Net;
using Mono.Nat;

namespace SpiritIsland.Web.Network;

/// <summary>
/// Finds the router with the Mono.Nat library (UPnP: it searches the network for an Internet Gateway Device and speaks its SOAP
/// interface). Not exercised by the tests, which use a fake router: it needs a real one.
/// </summary>
public sealed class MonoNatRouterFinder : IRouterFinder {

	public async Task<IRouter?> FindAsync( TimeSpan timeout, CancellationToken cancel ) {
		var found = new TaskCompletionSource<INatDevice>( TaskCreationOptions.RunContinuationsAsynchronously );
		void OnFound( object? sender, DeviceEventArgs e ) => found.TrySetResult( e.Device );

		NatUtility.DeviceFound += OnFound;
		try {
			NatUtility.StartDiscovery( NatProtocol.Upnp );
			INatDevice device = await found.Task.WaitAsync( timeout, cancel );
			return new MonoNatRouter( device );
		}
		catch( TimeoutException ) {
			return null;
		}
		finally {
			NatUtility.StopDiscovery();
			NatUtility.DeviceFound -= OnFound;
		}
	}
}

sealed class MonoNatRouter( INatDevice device ) : IRouter {

	public async Task<IPAddress?> GetExternalAddressAsync() => await device.GetExternalIPAsync();

	public async Task MapPortAsync( int port, TimeSpan lease, string description )
		=> await device.CreatePortMapAsync( new Mapping( Protocol.Tcp, port, port, (int)lease.TotalSeconds, description ) );

	public async Task UnmapPortAsync( int port )
		=> await device.DeletePortMapAsync( new Mapping( Protocol.Tcp, port, port ) );
}
