#!/usr/bin/env dotnet
// RouterDiagnostics.cs - a throwaway, single-file .NET 10 tool that finds out how an app on this LAN could be reached from the internet.
//
// RUN (needs the .NET 10 SDK; no project file, no NuGet packages, no administrator rights):
//     dotnet run tools/RouterDiagnostics.cs                           read-only diagnostics (changes nothing on the router)
//     dotnet run tools/RouterDiagnostics.cs -- --test-mapping 5080    also try a temporary TCP port mapping (changes the router, then undoes it)
//     dotnet run tools/RouterDiagnostics.cs -- --test-mapping 5080 --verify    ... and ask ifconfig.co whether the port is reachable from the internet
//     dotnet run tools/RouterDiagnostics.cs -- --help
// Options: --test-mapping <port>  --verify  --hold <seconds, default 20>  --lease <seconds, default 300>  --wait <seconds for SSDP, default 3>
//
// WHAT IT CHECKS (read-only): network interfaces and gateways; UPnP IGD discovery (SSDP multicast from EVERY IPv4 interface, and unicast to
// each gateway), device description, WANIP/WANPPP services, external IP, the port-mapping table (looking for TCP 32400, Plex); NAT-PMP;
// PCP; public IPv4/IPv6 and double-NAT/CGNAT; Plex (listening port, /identity, its Windows registry settings); then a summary and next steps.
// The only outside contact is an HTTPS "what is my IP" request (ipify / amazonaws / ifconfig.me) and, with --verify, ifconfig.co/port.
//
// STATUS WORDS: Supported = it answered correctly. Unsupported = it answered that it doesn't do this (or a port-unreachable came back).
// No Response = nothing came back before the timeout (a firewall, a different interface, or no such service). Failed = it answered with an
// error, or the attempt threw. Inconclusive = the evidence can't decide it, and the line says why.
//
// HOW TO READ IT
//  * "UPnP IGD found" only proves discovery. Mapping can still be refused (UPnP turned off for adds, 'only permanent leases', 718/725/728...).
//    --test-mapping is the real proof.
//  * SSDP "no response" on multicast but a reply on unicast = the router is fine; discovery code that sends one multicast on the default
//    interface misses it (VPN, Hyper-V/WSL/Docker adapters, several NICs, a firewall blocking the reply). Bind to each interface, try unicast.
//  * UPnP mapping tables only list what UPnP created. A forwarding rule typed into the router's admin page won't show, so "no 32400 entry"
//    does NOT mean Plex isn't forwarded, and it doesn't mean Plex is using Relay either.
//  * Router WAN address private (10/8, 172.16/12, 192.168/16) or 100.64/10 = double NAT or CGNAT: inbound from the internet won't arrive
//    however the mapping goes. A WAN address that differs from the public IP but is itself public is Inconclusive (VPN, multi-WAN, proxy).
//  * A test from inside the LAN can't prove internet reachability (many routers refuse "hairpin" connections). Use --verify or a phone on mobile data.
//  * Plex can reach you with no inbound rule at all: Plex Relay (limited bandwidth), or IPv6.
//
// ILLUSTRATIVE OUTPUT (made-up addresses, abridged):
//     == UPnP IGD ==
//       [Supported   ] multicast from 192.0.2.10 (Ethernet): 1 reply -> http://192.0.2.1:5000/rootDesc.xml
//       [Supported   ] WANIPConnection:1 external IP 203.0.113.7, status Connected
//       [Inconclusive] 3 mappings listed; no TCP 32400 (manual forwards and Relay don't appear here)
//     == NAT-PMP ==   [Unsupported ] 192.0.2.1: ICMP port unreachable
//     == PCP ==       [No Response ] 192.0.2.1
//     == NAT ==       [Supported   ] router WAN 203.0.113.7 = public IP 203.0.113.7: a single NAT
//
// Mapping test safety: it only adds a mapping on an external port that is verified free, never overwrites or deletes anything else, uses a
// short lease, and removes exactly what it added when it finishes or on Ctrl+C. The test listener answers every connection with one line
// of text, and runs only for the length of the test.

#:property PublishAot=false

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

Options opts;
try { opts = Options.Parse( args ); }
catch( ArgumentException ex ) { Console.Error.WriteLine( ex.Message ); Console.Error.WriteLine( Options.Usage ); return 2; }
if( opts.Help ) { Console.WriteLine( Options.Usage ); return 0; }

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += ( _, e ) => { e.Cancel = true; cts.Cancel(); Console.WriteLine( "\nCancelling (cleaning up)..." ); };
CancellationToken ct = cts.Token;

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds( 8 ) };
http.DefaultRequestHeaders.UserAgent.ParseAdd( "RouterDiagnostics/1.0" );

var ctx = new Context();
Out.Title( "Router / remote-access diagnostics" + ( opts.TestMappingPort is null ? " (read-only)" : "" ) );
try {
	Sections.Interfaces( ctx );
	await Sections.Upnp( ctx, opts, http, ct );
	await Sections.NatPmpAndPcp( ctx, ct );
	await Sections.PublicAndNat( ctx, http, ct );
	await Sections.Plex( ctx, http, ct );
	if( opts.TestMappingPort is int port ) await MappingTest.Run( port, ctx, opts, http, ct );
	Sections.Summary( ctx, opts );
}
catch( OperationCanceledException ) { Out.Info( "Cancelled." ); return 130; }
return 0;

// ------------------------------------------------------------------------------------------------------------------------------------

enum Status { Supported, Unsupported, NoResponse, Failed, Inconclusive }

record Nic( string Name, string Description, NetworkInterfaceType Type, bool Virtual, IPAddress? V4, int Prefix, List<IPAddress> V6, IPAddress? Gateway );
record GatewayInfo( IPAddress Address, IPAddress Local, string NicName, bool Primary );
record Service( string Type, string Id, Uri Control );
record PortMapping( string RemoteHost, int External, string Protocol, int Internal, string Client, bool Enabled, string Description, int Lease );

class IgdDevice {
	public required string Location { get; init; }
	public required IPAddress Local { get; init; }
	public required string Via { get; init; }
	public string? Server, Friendly, Manufacturer, Model, DeviceType;
	public List<Service> Services { get; } = [];
	public IEnumerable<Service> Connections => Services.Where( s => s.Type.Contains( "WANIPConnection" ) || s.Type.Contains( "WANPPPConnection" ) );
}

class Context {
	public List<Nic> Nics = [];
	public List<GatewayInfo> Gateways = [];
	public GatewayInfo? Primary => Gateways.FirstOrDefault( g => g.Primary );
	public List<IgdDevice> Igds = [];
	public bool MulticastFound, UnicastFound;
	public bool UpnpExternalIpWorked, UpnpMappingTableReadable;
	public List<(string Source, IPAddress Address)> RouterWan = [];
	public List<PortMapping> Mappings = [];
	public Dictionary<IPAddress, Status> NatPmp = [], Pcp = [];
	public IPAddress? PublicV4, PublicV6;
	public bool DoubleNat;
	public bool HasGlobalV6 => Nics.Any( n => n.V6.Count > 0 );
	public bool? PlexListening;
	public bool PlexMappedByUpnp;
	public string? PlexManualPortMode;
	public Status? MappingTest;
	public string? MappingTestDetail;
}

class Options {
	public bool Help, Verify;
	public int? TestMappingPort;
	public int Hold = 20, Lease = 300, Wait = 3;

	public const string Usage = """
		RouterDiagnostics [--test-mapping <port>] [--verify] [--hold <s>] [--lease <s>] [--wait <s>] [--help]
		  (no options)          read-only diagnostics; nothing on the router is changed
		  --test-mapping <port> after the diagnostics, add a TEMPORARY TCP mapping for <port> (UPnP, then PCP, then NAT-PMP), then remove it
		  --verify              with --test-mapping: ask ifconfig.co/port whether the port is reachable from the internet (sends it your IP)
		  --hold <s>            how long the mapping stays up for the test (default 20)
		  --lease <s>           lease requested from the router (default 300; it expires by itself if cleanup fails)
		  --wait <s>            how long to listen for SSDP replies (default 3)
		""";

	public static Options Parse( string[] args ) {
		var o = new Options();
		for( int i = 0; i < args.Length; ++i ) {
			int Number() => i + 1 < args.Length && int.TryParse( args[++i], out int n ) && n >= 0 ? n : throw new ArgumentException( $"{args[i - 1]} needs a number" );
			switch( args[i] ) {
				case "--help" or "-h" or "-?": o.Help = true; break;
				case "--verify": o.Verify = true; break;
				case "--test-mapping": int p = Number(); o.TestMappingPort = p is > 0 and < 65536 ? p : throw new ArgumentException( "port must be 1-65535" ); break;
				case "--hold": o.Hold = Number(); break;
				case "--lease": o.Lease = Number(); break;
				case "--wait": o.Wait = Math.Max( 1, Number() ); break;
				default: throw new ArgumentException( $"unknown option {args[i]}" );
			}
		}
		if( o.Verify && o.TestMappingPort is null ) throw new ArgumentException( "--verify only makes sense with --test-mapping" );
		return o;
	}
}

static class Out {
	static readonly Dictionary<Status, (string Label, ConsoleColor Color)> Looks = new() {
		[Status.Supported] = ("Supported", ConsoleColor.Green), [Status.Unsupported] = ("Unsupported", ConsoleColor.DarkYellow),
		[Status.NoResponse] = ("No Response", ConsoleColor.DarkYellow), [Status.Failed] = ("Failed", ConsoleColor.Red),
		[Status.Inconclusive] = ("Inconclusive", ConsoleColor.Yellow),
	};
	public static void Title( string t ) { Colored( ConsoleColor.Cyan, $"{t}\n{new string( '=', t.Length )}" ); }
	public static void Section( string t ) { Console.WriteLine(); Colored( ConsoleColor.Cyan, $"== {t} ==" ); }
	public static void Row( Status s, string text, int indent = 2 ) {
		var (label, color) = Looks[s];
		Console.Write( new string( ' ', indent ) + "[" );
		Colored( color, label.PadRight( 12 ), newline: false );
		Console.WriteLine( "] " + text );
	}
	public static void Info( string text, int indent = 2 ) => Console.WriteLine( new string( ' ', indent ) + text );
	static void Colored( ConsoleColor c, string text, bool newline = true ) {
		Console.ForegroundColor = c;
		if( newline ) Console.WriteLine( text ); else Console.Write( text );
		Console.ResetColor();
	}
}

static class Net {
	public static string Classify( IPAddress a ) {
		if( a.AddressFamily != AddressFamily.InterNetwork ) return "ipv6";
		byte[] b = a.GetAddressBytes();
		return b[0] switch {
			0 => "unspecified", 127 => "loopback", 10 => "private",
			172 when b[1] is >= 16 and <= 31 => "private", 192 when b[1] == 168 => "private",
			100 when b[1] is >= 64 and <= 127 => "cgnat", 169 when b[1] == 254 => "link-local",
			_ => "public"
		};
	}

	public static bool IsGlobalV6( IPAddress a )
		=> a.AddressFamily == AddressFamily.InterNetworkV6 && (a.GetAddressBytes()[0] & 0xE0) == 0x20; // 2000::/3; excludes link-local, ULA, loopback

	/// <summary> Sends datagrams to a UDP endpoint with a connected socket and waits for one reply, retrying with doubling timeouts (250, 500, 1000 ms). </summary>
	public static async Task<(byte[]? Data, string? Error)> Exchange( IPAddress local, IPAddress remote, int remotePort, byte[] request, CancellationToken ct ) {
		using var udp = new UdpClient( new IPEndPoint( local, 0 ) );
		udp.Connect( remote, remotePort );
		int delay = 250;
		for( int attempt = 0; attempt < 3; ++attempt ) {
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource( ct );
			timeout.CancelAfter( delay );
			try {
				await udp.SendAsync( request, ct );
				return ((await udp.ReceiveAsync( timeout.Token )).Buffer, null);
			}
			catch( OperationCanceledException ) when( !ct.IsCancellationRequested ) { delay *= 2; }
			catch( SocketException ex ) { return (null, $"{ex.SocketErrorCode} - {ex.Message}"); } // typically ICMP port unreachable
		}
		return (null, null);
	}

	public static async Task<IPAddress?> FetchIp( HttpClient http, string[] urls, AddressFamily family, CancellationToken ct, List<string> notes ) {
		foreach( string url in urls ) {
			try {
				string text = ( await http.GetStringAsync( url, ct ) ).Trim();
				if( IPAddress.TryParse( text, out IPAddress? ip ) && ip.AddressFamily == family ) return ip;
				notes.Add( $"{url} returned '{Trunc( text )}', not an address of the right kind" );
			}
			catch( Exception ex ) when( ex is not OperationCanceledException || !ct.IsCancellationRequested ) { notes.Add( $"{url}: {ex.GetType().Name} {ex.InnerException?.Message ?? ex.Message}" ); }
		}
		return null;
	}

	public static string Trunc( string s, int n = 120 ) => s.Length <= n ? s : s[..n] + "...";
}

static class UpnpClient {
	const string Multicast = "239.255.255.250";
	public static readonly string[] SearchTargets = [
		"urn:schemas-upnp-org:device:InternetGatewayDevice:1", "urn:schemas-upnp-org:device:InternetGatewayDevice:2", "upnp:rootdevice" ];

	/// <summary> Sends M-SEARCH for each target from <paramref name="local"/> and collects every reply for <paramref name="wait"/>. </summary>
	public static async Task<(List<Dictionary<string, string>> Replies, string? Error)> Search( IPAddress local, IPEndPoint target, TimeSpan wait, CancellationToken ct ) {
		var replies = new List<Dictionary<string, string>>();
		try {
			using var udp = new UdpClient( new IPEndPoint( local, 0 ) );
			if( target.Address.Equals( IPAddress.Parse( Multicast ) ) ) {
				udp.Client.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.MulticastInterface, local.GetAddressBytes() );
				udp.Client.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2 );
			}
			foreach( string st in SearchTargets ) {
				byte[] msg = Encoding.ASCII.GetBytes( $"M-SEARCH * HTTP/1.1\r\nHOST: {target}\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: {st}\r\n\r\n" );
				await udp.SendAsync( msg, msg.Length, target );
			}
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource( ct );
			timeout.CancelAfter( wait );
			while( true ) {
				try {
					UdpReceiveResult r = await udp.ReceiveAsync( timeout.Token );
					var headers = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ) { ["_from"] = r.RemoteEndPoint.Address.ToString() };
					foreach( string line in Encoding.UTF8.GetString( r.Buffer ).Split( "\r\n" ) ) {
						int colon = line.IndexOf( ':' );
						if( colon > 0 ) headers[line[..colon].Trim()] = line[( colon + 1 )..].Trim();
					}
					replies.Add( headers );
				}
				catch( OperationCanceledException ) when( !ct.IsCancellationRequested ) { break; }
				catch( SocketException ex ) when( ex.SocketErrorCode == SocketError.ConnectionReset ) { } // ICMP unreachable for one of our sends; keep listening
			}
			return (replies, null);
		}
		catch( SocketException ex ) { return (replies, $"{ex.SocketErrorCode} - {ex.Message}"); }
	}

	public static async Task<IgdDevice?> Describe( string location, IPAddress local, string via, string? server, HttpClient http, CancellationToken ct ) {
		var device = new IgdDevice { Location = location, Local = local, Via = via, Server = server };
		try {
			XDocument doc = XDocument.Parse( await http.GetStringAsync( location, ct ) );
			static string? Child( XElement? e, string name ) => e?.Elements().FirstOrDefault( c => c.Name.LocalName == name )?.Value.Trim();
			XElement? root = doc.Descendants().FirstOrDefault( e => e.Name.LocalName == "device" );
			device.Friendly = Child( root, "friendlyName" ); device.Manufacturer = Child( root, "manufacturer" );
			device.Model = Child( root, "modelName" ); device.DeviceType = Child( root, "deviceType" );
			string? urlBase = Child( doc.Root, "URLBase" );
			var baseUri = new Uri( string.IsNullOrEmpty( urlBase ) ? location : urlBase );
			foreach( XElement s in doc.Descendants().Where( e => e.Name.LocalName == "service" ) ) {
				string? type = Child( s, "serviceType" ), control = Child( s, "controlURL" );
				if( type is not null && control is not null ) device.Services.Add( new Service( type, Child( s, "serviceId" ) ?? "", new Uri( baseUri, control ) ) );
			}
			return device;
		}
		catch( Exception ex ) when( ex is not OperationCanceledException || !ct.IsCancellationRequested ) {
			Out.Row( Status.Failed, $"could not read the description at {location}: {ex.GetType().Name} {ex.Message}", 4 );
			return null;
		}
	}

	/// <summary> One SOAP call. On a UPnP error the second item holds 'code description'. </summary>
	public static async Task<(Dictionary<string, string>? Values, string? Error)> Soap( HttpClient http, Service svc, string action, (string Name, string Value)[] args, CancellationToken ct ) {
		string arguments = string.Concat( args.Select( a => $"<{a.Name}>{SecurityElement.Escape( a.Value )}</{a.Name}>" ) );
		string body = $"<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
			$"<s:Body><u:{action} xmlns:u=\"{svc.Type}\">{arguments}</u:{action}></s:Body></s:Envelope>";
		using var request = new HttpRequestMessage( HttpMethod.Post, svc.Control ) { Content = new StringContent( body, Encoding.UTF8, "text/xml" ) };
		request.Headers.TryAddWithoutValidation( "SOAPACTION", $"\"{svc.Type}#{action}\"" );
		try {
			using HttpResponseMessage response = await http.SendAsync( request, ct );
			string text = await response.Content.ReadAsStringAsync( ct );
			XDocument? doc = null;
			try { doc = XDocument.Parse( text ); } catch( System.Xml.XmlException ) { }
			if( !response.IsSuccessStatusCode ) {
				string? code = doc?.Descendants().FirstOrDefault( e => e.Name.LocalName == "errorCode" )?.Value;
				string? desc = doc?.Descendants().FirstOrDefault( e => e.Name.LocalName == "errorDescription" )?.Value;
				return (null, code is null ? $"HTTP {(int)response.StatusCode} {Net.Trunc( text, 80 )}" : $"{code} {desc}");
			}
			XElement? result = doc?.Descendants().FirstOrDefault( e => e.Name.LocalName == action + "Response" );
			return (result?.Elements().ToDictionary( e => e.Name.LocalName, e => e.Value.Trim() ) ?? [], null);
		}
		catch( Exception ex ) when( ex is not OperationCanceledException || !ct.IsCancellationRequested ) { return (null, $"{ex.GetType().Name} {ex.Message}"); }
	}

	public static PortMapping ToMapping( Dictionary<string, string> v ) {
		string Get( string k ) => v.GetValueOrDefault( k, "" );
		int Num( string k ) => int.TryParse( Get( k ), out int n ) ? n : 0;
		return new PortMapping( Get( "NewRemoteHost" ), Num( "NewExternalPort" ), Get( "NewProtocol" ), Num( "NewInternalPort" ), Get( "NewInternalClient" ),
			Get( "NewEnabled" ) is "1" or "true", Get( "NewPortMappingDescription" ), Num( "NewLeaseDuration" ) );
	}

	public static (string, string)[] PortArgs( int external, string protocol = "TCP" )
		=> [("NewRemoteHost", ""), ("NewExternalPort", external.ToString()), ("NewProtocol", protocol)];
}

static class Sections {

	public static void Interfaces( Context c ) {
		Out.Section( "Network interfaces and gateways" );
		foreach( NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces() ) {
			if( ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ) continue;
			IPInterfaceProperties props = ni.GetIPProperties();
			UnicastIPAddressInformation? v4 = props.UnicastAddresses.FirstOrDefault( u => u.Address.AddressFamily == AddressFamily.InterNetwork );
			if( v4 is null && !props.UnicastAddresses.Any( u => Net.IsGlobalV6( u.Address ) ) ) continue; // Windows lists many filter-driver pseudo-adapters with no usable address
			IPAddress? gw = props.GatewayAddresses.Select( g => g.Address ).FirstOrDefault( a => a.AddressFamily == AddressFamily.InterNetwork && !a.Equals( IPAddress.Any ) );
			bool isVirtual = Regex.IsMatch( ni.Name + " " + ni.Description, "vEthernet|Hyper-V|VirtualBox|VMware|WSL|Docker|TAP|TUN|VPN|Virtual|Bluetooth|Tailscale|ZeroTier|Teredo", RegexOptions.IgnoreCase );
			var nic = new Nic( ni.Name, ni.Description, ni.NetworkInterfaceType, isVirtual, v4?.Address, v4?.PrefixLength ?? 0,
				[.. props.UnicastAddresses.Select( u => u.Address ).Where( Net.IsGlobalV6 )], gw );
			c.Nics.Add( nic );

			Out.Info( $"{ni.Name} - {ni.Description} ({ni.NetworkInterfaceType}){( isVirtual ? " [looks virtual/VPN]" : "" )}" );
			Out.Info( $"IPv4 {( v4 is null ? "none" : $"{v4.Address}/{v4.PrefixLength}" )}   gateway {gw?.ToString() ?? "none"}", 4 );
			foreach( IPAddress a in props.UnicastAddresses.Select( u => u.Address ).Where( a => a.AddressFamily == AddressFamily.InterNetworkV6 && !a.IsIPv6LinkLocal ) )
				Out.Info( $"IPv6 {a}{( Net.IsGlobalV6( a ) ? "  (globally routable)" : "  (not global)" )}", 4 );
		}

		// Gateways: one per interface that has an IPv4 address and an IPv4 gateway. The most likely router is the first on a non-virtual interface.
		var withGateway = c.Nics.Where( n => n.Gateway is not null && n.V4 is not null ).ToList();
		Nic? primary = withGateway.FirstOrDefault( n => !n.Virtual ) ?? withGateway.FirstOrDefault();
		foreach( Nic n in withGateway )
			if( !c.Gateways.Any( g => g.Address.Equals( n.Gateway ) ) ) c.Gateways.Add( new GatewayInfo( n.Gateway!, n.V4!, n.Name, n == primary ) );
		Console.WriteLine();
		if( c.Gateways.Count == 0 ) Out.Row( Status.Failed, "no IPv4 default gateway found; is this computer on a network?" );
		else {
			foreach( GatewayInfo g in c.Gateways ) Out.Info( $"{( g.Primary ? "Most likely router" : "Other gateway" )}: {g.Address} (reached from {g.Local} on {g.NicName})" );
			if( c.Gateways.Count > 1 ) Out.Info( "More than one gateway: a VPN or virtual adapter is present. Each is probed below, so any of them may be the one that answers." );
		}
	}

	public static async Task Upnp( Context c, Options o, HttpClient http, CancellationToken ct ) {
		Out.Section( "UPnP IGD discovery" );
		TimeSpan wait = TimeSpan.FromSeconds( o.Wait );
		var found = new Dictionary<string, (IPAddress Local, string Via, string? Server)>();

		// SSDP multicast from every IPv4 interface, because the reply only comes back to the interface that asked, and "the default one" is often wrong.
		var multicast = new IPEndPoint( IPAddress.Parse( "239.255.255.250" ), 1900 );
		IPAddress[] locals = [.. c.Nics.Where( n => n.V4 is not null && Net.Classify( n.V4 ) != "link-local" ).Select( n => n.V4! )];
		var multicastRuns = await Task.WhenAll( locals.Select( async l => (Local: l, Result: await Sddp( l, multicast )) ) );
		foreach( var (local, (replies, error)) in multicastRuns ) {
			string nic = c.Nics.First( n => local.Equals( n.V4 ) ).Name;
			if( error is not null ) Out.Row( Status.Failed, $"multicast from {local} ({nic}): {error}" );
			else if( replies.Count == 0 ) Out.Row( Status.NoResponse, $"multicast to 239.255.255.250:1900 from {local} ({nic}): nobody replied in {o.Wait}s" );
			else {
				Out.Row( Status.Supported, $"multicast from {local} ({nic}): {replies.Count} replies from {string.Join( ", ", replies.Select( r => r["_from"] ).Distinct() )}" );
				foreach( var r in replies.Where( r => r.ContainsKey( "LOCATION" ) ) ) found.TryAdd( r["LOCATION"], (local, "multicast", r.GetValueOrDefault( "SERVER" )) );
			}
		}

		// SSDP unicast straight to each gateway. Many routers answer this even when multicast never reaches them (or never gets back).
		foreach( GatewayInfo g in c.Gateways ) {
			var (replies, error) = await Sddp( g.Local, new IPEndPoint( g.Address, 1900 ) );
			if( error is not null ) Out.Row( Status.Failed, $"unicast to {g.Address}:1900: {error}" );
			else if( replies.Count == 0 ) Out.Row( Status.NoResponse, $"unicast to gateway {g.Address}:1900: nothing came back" );
			else {
				Out.Row( Status.Supported, $"unicast to gateway {g.Address}:1900: {replies.Count} replies" );
				foreach( var r in replies.Where( r => r.ContainsKey( "LOCATION" ) ) ) found.TryAdd( r["LOCATION"], (g.Local, "unicast", r.GetValueOrDefault( "SERVER" )) );
			}
		}

		Task<(List<Dictionary<string, string>> Replies, string? Error)> Sddp( IPAddress local, IPEndPoint target ) => UpnpClient.Search( local, target, wait, ct );

		// Describe each distinct device; keep only Internet Gateway Devices.
		var described = await Task.WhenAll( found.Select( f => UpnpClient.Describe( f.Key, f.Value.Local, f.Value.Via, f.Value.Server, http, ct ) ) );
		foreach( IgdDevice? d in described.Where( d => d is not null ) ) {
			bool isIgd = ( d!.DeviceType ?? "" ).Contains( "InternetGatewayDevice" ) || d.Services.Any( s => s.Type.Contains( "WANIPConnection" ) || s.Type.Contains( "WANPPPConnection" ) );
			if( isIgd ) c.Igds.Add( d );
		}
		c.MulticastFound = c.Igds.Any( d => d.Via == "multicast" );
		c.UnicastFound = c.Igds.Any( d => d.Via == "unicast" );
		int others = described.Count( d => d is not null ) - c.Igds.Count;
		if( others > 0 ) Out.Info( $"({others} other UPnP device(s) answered too, for example TVs or media servers; ignored.)" );

		if( c.Igds.Count == 0 ) {
			Console.WriteLine();
			Out.Row( Status.NoResponse, "no UPnP Internet Gateway Device was found, by multicast or unicast. Either UPnP is off in the router, a firewall (Windows, VPN, security software) is dropping SSDP, or the router only supports NAT-PMP/PCP/manual forwarding." );
			return;
		}
		if( c.UnicastFound && !c.MulticastFound )
			Out.Row( Status.Inconclusive, "the router answers unicast SSDP but no multicast probe got a reply: multicast discovery code (one packet on the default interface) would miss it. Look at the firewall, VPN and virtual adapters." );

		foreach( IgdDevice d in c.Igds ) {
			Console.WriteLine();
			Out.Row( Status.Supported, $"IGD {d.Friendly ?? "(unnamed)"} - {d.Manufacturer} {d.Model} (found by {d.Via} from {d.Local})" );
			Out.Info( $"description {d.Location}", 6 );
			if( d.Server is not null ) Out.Info( $"SERVER: {d.Server}", 6 );
			foreach( Service s in d.Services ) Out.Info( $"service {s.Type}  control {s.Control.PathAndQuery}", 6 );
			if( !d.Connections.Any() ) { Out.Row( Status.Unsupported, "no WANIPConnection or WANPPPConnection service listed", 4 ); continue; }
			foreach( Service s in d.Connections ) await QueryConnection( c, http, d, s, ct );
		}
	}

	static async Task QueryConnection( Context c, HttpClient http, IgdDevice d, Service s, CancellationToken ct ) {
		string kind = s.Type[( s.Type.LastIndexOf( ':', s.Type.LastIndexOf( ':' ) - 1 ) + 1 )..];
		Out.Info( $"-- {kind} --", 4 );

		var (ext, extError) = await UpnpClient.Soap( http, s, "GetExternalIPAddress", [], ct );
		if( ext is not null && ext.TryGetValue( "NewExternalIPAddress", out string? ipText ) && IPAddress.TryParse( ipText, out IPAddress? wan ) ) {
			c.UpnpExternalIpWorked = true;
			if( wan.Equals( IPAddress.Any ) ) Out.Row( Status.Inconclusive, "GetExternalIPAddress returned 0.0.0.0 (the router's WAN link is probably down)", 4 );
			else { Out.Row( Status.Supported, $"GetExternalIPAddress = {wan} ({Net.Classify( wan )})", 4 ); c.RouterWan.Add( ($"UPnP {d.Friendly}", wan) ); }
		}
		else Out.Row( Status.Failed, $"GetExternalIPAddress: {extError ?? "no address in the reply"}", 4 );

		var (status, statusError) = await UpnpClient.Soap( http, s, "GetStatusInfo", [], ct );
		if( status is not null ) Out.Info( $"connection status: {status.GetValueOrDefault( "NewConnectionStatus" )}, uptime {status.GetValueOrDefault( "NewUptime" )}s", 4 );
		else Out.Info( $"GetStatusInfo failed: {statusError}", 4 );

		// Port-mapping table: index until the router says the index is invalid (713). Capped, and read-only.
		var mappings = new List<PortMapping>();
		string? tableError = null;
		for( int i = 0; i < 256; ++i ) {
			var (entry, error) = await UpnpClient.Soap( http, s, "GetGenericPortMappingEntry", [("NewPortMappingIndex", i.ToString())], ct );
			if( entry is null ) { if( !( error ?? "" ).StartsWith( "713" ) ) tableError = error; break; }
			mappings.Add( UpnpClient.ToMapping( entry ) );
		}
		if( tableError is not null ) Out.Row( Status.Failed, $"port-mapping table could not be read: {tableError}. (Many routers hide it; that says nothing about whether adding works.)", 4 );
		else {
			c.UpnpMappingTableReadable = true;
			c.Mappings.AddRange( mappings );
			Out.Row( mappings.Any( m => m.External == 32400 && m.Protocol == "TCP" ) ? Status.Supported : Status.Inconclusive,
				$"{mappings.Count} port mapping(s) in the table{( mappings.Count == 0 ? "" : ":" )}", 4 );
			foreach( PortMapping m in mappings )
				Out.Info( $"{m.Protocol} external {m.External} -> {m.Client}:{m.Internal}  {( m.Enabled ? "" : "(disabled) " )}lease {m.Lease}s  '{m.Description}'{( m.External == 32400 || m.Internal == 32400 ? "   <-- Plex" : "" )}", 6 );
		}

		// Ask for 32400 directly: some routers answer this even when they refuse to list the table.
		var (specific, specificError) = await UpnpClient.Soap( http, s, "GetSpecificPortMappingEntry", UpnpClient.PortArgs( 32400 ), ct );
		if( specific is not null ) {
			PortMapping m = UpnpClient.ToMapping( specific );
			c.PlexMappedByUpnp = true;
			Out.Row( Status.Supported, $"TCP 32400 is mapped: -> {specific.GetValueOrDefault( "NewInternalClient" )}:{specific.GetValueOrDefault( "NewInternalPort" )} lease {m.Lease}s '{m.Description}'", 4 );
		}
		else if( ( specificError ?? "" ).StartsWith( "714" ) ) Out.Row( Status.Inconclusive, "no UPnP mapping for TCP 32400 (714 NoSuchEntryInArray). A manually configured forward, IPv6 or Plex Relay would not appear here.", 4 );
		else Out.Row( Status.Failed, $"GetSpecificPortMappingEntry 32400: {specificError}", 4 );
	}

	public static async Task NatPmpAndPcp( Context c, CancellationToken ct ) {
		Out.Section( "NAT-PMP (RFC 6886) and PCP (RFC 6887), UDP 5351" );
		if( c.Gateways.Count == 0 ) { Out.Row( Status.Inconclusive, "no gateway to probe" ); return; }
		foreach( GatewayInfo g in c.Gateways ) {
			Out.Info( $"gateway {g.Address} (from {g.Local})" );

			// NAT-PMP: version 0, opcode 0 = "what is my external address?"
			var (np, npError) = await Net.Exchange( g.Local, g.Address, 5351, [0, 0], ct );
			Status npStatus;
			if( np is null ) { npStatus = npError is null ? Status.NoResponse : Status.Unsupported; Out.Row( npStatus, npError is null ? "NAT-PMP: no reply in ~1.75 s" : $"NAT-PMP: {npError}", 4 ); }
			else if( np.Length >= 12 && np[0] == 0 && np[1] == 128 ) {
				int code = np[2] << 8 | np[3];
				if( code == 0 ) {
					var ext = new IPAddress( np[8..12] );
					npStatus = Status.Supported;
					Out.Row( npStatus, $"NAT-PMP: external address {ext} ({Net.Classify( ext )}), epoch {( np[4] << 24 | np[5] << 16 | np[6] << 8 | np[7] )}s", 4 );
					if( !ext.Equals( IPAddress.Any ) ) c.RouterWan.Add( ("NAT-PMP", ext) );
				}
				else { npStatus = code == 1 ? Status.Unsupported : Status.Failed; Out.Row( npStatus, $"NAT-PMP: result code {code} ({NatPmpCode( code )})", 4 ); }
			}
			else { npStatus = Status.Failed; Out.Row( npStatus, $"NAT-PMP: unexpected reply {Convert.ToHexString( np )}", 4 ); }
			c.NatPmp[g.Address] = npStatus;

			// PCP: version 2, opcode 0 = ANNOUNCE (no state change). A NAT-PMP-only router answers with version 0, which tells the two apart.
			byte[] announce = new byte[24];
			announce[0] = 2;
			g.Local.MapToIPv6().GetAddressBytes().CopyTo( announce, 8 );
			var (pcp, pcpError) = await Net.Exchange( g.Local, g.Address, 5351, announce, ct );
			Status pcpStatus;
			if( pcp is null ) { pcpStatus = pcpError is null ? Status.NoResponse : Status.Unsupported; Out.Row( pcpStatus, pcpError is null ? "PCP: no reply in ~1.75 s" : $"PCP: {pcpError}", 4 ); }
			else if( pcp.Length >= 24 && pcp[0] == 2 && pcp[1] == 0x80 ) {
				pcpStatus = pcp[3] == 0 ? Status.Supported : Status.Failed;
				Out.Row( pcpStatus, $"PCP: version 2 reply, result code {pcp[3]}{( pcp[3] == 0 ? " (success)" : "" )}", 4 );
			}
			else if( pcp.Length >= 8 && pcp[0] == 0 && pcp[1] == 0x80 ) {
				pcpStatus = Status.Unsupported;
				Out.Row( pcpStatus, $"PCP: the router answered in NAT-PMP format (version 0, result {pcp[2] << 8 | pcp[3]}), so it speaks NAT-PMP only", 4 );
			}
			else { pcpStatus = Status.Failed; Out.Row( pcpStatus, $"PCP: unexpected reply {Convert.ToHexString( pcp )}", 4 ); }
			c.Pcp[g.Address] = pcpStatus;
		}
	}

	static string NatPmpCode( int code ) => code switch {
		1 => "unsupported version", 2 => "not authorized / refused", 3 => "network failure", 4 => "out of resources", 5 => "unsupported opcode", _ => "unknown" };

	public static async Task PublicAndNat( Context c, HttpClient http, CancellationToken ct ) {
		Out.Section( "Public IP, NAT and IPv6" );
		var notes = new List<string>();
		c.PublicV4 = await Net.FetchIp( http, ["https://api.ipify.org", "https://checkip.amazonaws.com", "https://ifconfig.me/ip"], AddressFamily.InterNetwork, ct, notes );
		if( c.PublicV4 is null ) Out.Row( Status.Failed, "could not learn the public IPv4 address: " + string.Join( "; ", notes ) );
		else Out.Row( Status.Supported, $"public IPv4 as the internet sees this network: {c.PublicV4}" );

		var distinctWan = c.RouterWan.GroupBy( w => w.Address ).Select( g => g.First() ).ToList();
		if( distinctWan.Count == 0 )
			Out.Row( Status.Inconclusive, "the router's WAN address is unknown (UPnP and NAT-PMP didn't give one), so double NAT and CGNAT can't be ruled in or out from here. Compare the WAN address on the router's status page with the public IP above: if they differ, you're behind another NAT." );
		foreach( var (source, wan) in distinctWan ) {
			string kind = Net.Classify( wan );
			if( kind is "private" or "cgnat" ) {
				c.DoubleNat = true;
				Out.Row( Status.Failed, $"router WAN address {wan} ({source}) is {( kind == "cgnat" ? "in the carrier-grade NAT range 100.64.0.0/10" : "a private address" )}: a double NAT or CGNAT. Inbound connections from the internet cannot reach this router, whatever port mapping succeeds." );
			}
			else if( c.PublicV4 is null ) Out.Row( Status.Inconclusive, $"router WAN address {wan} ({source}) is public, but the public IP lookup failed, so they can't be compared" );
			else if( wan.Equals( c.PublicV4 ) ) Out.Row( Status.Supported, $"router WAN address {wan} ({source}) equals the public IP: a single NAT, so inbound mappings can work" );
			else Out.Row( Status.Inconclusive, $"router WAN address {wan} ({source}) is public but differs from the public IP {c.PublicV4}: a VPN, proxy, several WAN links or another translation. Can't call it." );
		}

		Console.WriteLine();
		var globalV6 = c.Nics.SelectMany( n => n.V6.Select( a => (n.Name, a) ) ).ToList();
		var notes6 = new List<string>();
		c.PublicV6 = await Net.FetchIp( http, ["https://api6.ipify.org", "https://v6.ident.me"], AddressFamily.InterNetworkV6, ct, notes6 );
		if( globalV6.Count == 0 && c.PublicV6 is null ) Out.Row( Status.Unsupported, "no globally routable IPv6 address on this computer and no IPv6 route to the internet" );
		else if( globalV6.Count == 0 ) Out.Row( Status.Inconclusive, $"the internet sees {c.PublicV6} but this computer has no global address on an interface it reports (a tunnel or a proxy?)" );
		else {
			foreach( var (name, a) in globalV6 ) Out.Row( Status.Supported, $"global IPv6 {a} on {name}" );
			if( c.PublicV6 is null ) Out.Row( Status.Inconclusive, "an IPv6 address exists but an IPv6 request to the internet failed: " + string.Join( "; ", notes6 ) );
			else Out.Row( Status.Supported, $"IPv6 reaches the internet from {c.PublicV6}. Inbound IPv6 needs the router's IPv6 firewall to allow the port; there is no port mapping to do (unless PCP/UPnP pinholes are offered)." );
		}
	}

	public static async Task Plex( Context c, HttpClient http, CancellationToken ct ) {
		Out.Section( "Plex (TCP 32400)" );
		var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Where( e => e.Port == 32400 ).ToList();
		c.PlexListening = listeners.Count > 0;
		if( listeners.Count > 0 ) Out.Row( Status.Supported, $"something is listening on TCP 32400: {string.Join( ", ", listeners.Select( l => l.ToString() ) )}" );
		else Out.Row( Status.Unsupported, "nothing on this computer is listening on TCP 32400. Plex may run on another machine, or it's stopped. To see how Plex is exposed, run this tool on the machine that runs Plex." );

		try {
			string identity = await http.GetStringAsync( "http://127.0.0.1:32400/identity", ct );
			Match m = Regex.Match( identity, "version=\"([^\"]+)\"" );
			Out.Row( Status.Supported, $"GET http://127.0.0.1:32400/identity answered{( m.Success ? $" (Plex Media Server {m.Groups[1].Value})" : "" )}" );
		}
		catch( Exception ex ) when( ex is not OperationCanceledException || !ct.IsCancellationRequested ) { Out.Row( Status.NoResponse, $"GET /identity on 127.0.0.1:32400: {ex.GetType().Name}" ); }

		if( OperatingSystem.IsWindows() ) ReadPlexRegistry( c );
		else Out.Info( "Plex's own settings are read from the Windows registry only; check Settings > Remote Access instead." );

		Console.WriteLine();
		Out.Info( "What this does and doesn't show:" );
		Out.Row( c.PlexMappedByUpnp ? Status.Supported : Status.Inconclusive,
			c.PlexMappedByUpnp ? "a UPnP mapping for 32400 exists: automatic mapping (or a manual UPnP entry) is in use" :
			"no UPnP mapping for 32400 is visible. That fits a manual forwarding rule on the router (not listed by UPnP), IPv6, or Plex Relay - and also a router that hides its UPnP table. It does not mean Relay." );
		if( c.PlexManualPortMode == "1" ) Out.Info( "Plex is set to 'Manually specify public port': Plex is NOT asking the router for a mapping; the router must have a forward you made by hand." );
		if( c.HasGlobalV6 ) Out.Info( "This computer has a global IPv6 address, so Plex may also be reachable over IPv6 with no port mapping." );
		Out.Info( "To settle it, open Plex > Settings > Remote Access: it shows 'Fully accessible outside your network' (direct), 'Indirect' (Relay), the public port, and the manual-port setting." );
	}

	static void ReadPlexRegistry( Context c ) {
		if( !OperatingSystem.IsWindows() ) return;
		try {
			using Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( @"Software\Plex, Inc.\Plex Media Server" );
			if( key is null ) { Out.Row( Status.Inconclusive, "no Plex Media Server settings in this user's registry (a different account, a service, or not installed here)" ); return; }
			var interesting = key.GetValueNames().Where( n => Regex.IsMatch( n, "port|mapping|relay|remote|upnp|ipv6|publish|custom|secure", RegexOptions.IgnoreCase ) ).Order().ToList();
			Out.Row( Status.Supported, $"Plex settings in the registry ({interesting.Count} related to networking):" );
			foreach( string name in interesting ) {
				Out.Info( $"{name} = {key.GetValue( name )}", 6 );
				if( name == "ManualPortMappingMode" ) c.PlexManualPortMode = key.GetValue( name )?.ToString();
			}
		}
		catch( Exception ex ) { Out.Row( Status.Failed, $"reading Plex's registry settings: {ex.Message}" ); }
	}

	public static void Summary( Context c, Options o ) {
		Out.Section( "Summary and next steps for exposing TCP 5080" );
		bool igd = c.Igds.Any( d => d.Connections.Any() );
		bool natPmp = c.NatPmp.Values.Any( s => s == Status.Supported ), pcp = c.Pcp.Values.Any( s => s == Status.Supported );
		Out.Info( $"UPnP IGD: {( igd ? $"found ({( c.MulticastFound ? "multicast" : "unicast only" )})" : "not found" )}   NAT-PMP: {( natPmp ? "yes" : "no" )}   PCP: {( pcp ? "yes" : "no" )}   double NAT/CGNAT: {( c.DoubleNat ? "LIKELY" : "not indicated" )}   global IPv6: {( c.HasGlobalV6 ? "yes" : "no" )}" );
		Console.WriteLine();

		if( c.DoubleNat ) {
			Out.Row( Status.Failed, "Port mapping on this router can't make the app reachable from the internet: there is another NAT above it." );
			Out.Info( "Options: put the upstream device in bridge mode, forward the port on the upstream device too, ask the ISP for a public (non-CGNAT) IP, use IPv6 if available, or use an outbound tunnel / relay (Cloudflare Tunnel, Tailscale Funnel, ngrok).", 6 );
		}
		else if( igd || natPmp || pcp ) {
			string[] ways = [.. new[] { igd ? "UPnP AddPortMapping" : null, pcp ? "PCP MAP" : null, natPmp ? "NAT-PMP mapping" : null }.OfType<string>()];
			Out.Row( Status.Inconclusive, $"Automatic mapping may be possible via {string.Join( ", ", ways )}. Discovery doesn't prove the router will accept a mapping." );
			Out.Info( o.TestMappingPort is null ? "Run again with --test-mapping 5080 (add --verify) to find out for sure." : "See the mapping test above for the proof.", 6 );
			if( igd && !c.MulticastFound ) Out.Info( "Your app's UPnP discovery should send SSDP from every IPv4 interface and also unicast to the default gateway; multicast alone missed this router.", 6 );
		}
		else {
			Out.Row( Status.NoResponse, "No automatic mapping protocol answered." );
			Out.Info( "If your router has UPnP, check it's switched on, then check Windows Firewall / security software / VPN for blocked UDP 1900.", 6 );
			Out.Info( "Otherwise: add a manual forward for TCP 5080 to this computer in the router admin page (give this PC a fixed/reserved LAN address first), or use IPv6, or an outbound tunnel.", 6 );
		}
		if( o.TestMappingPort is not null && c.MappingTest is not null )
			Out.Row( c.MappingTest.Value, $"mapping test: {c.MappingTestDetail}" );

		Console.WriteLine();
		Out.Info( "For any of these: the app must listen on 0.0.0.0 (not localhost) and Windows Firewall must allow inbound TCP 5080. Test from a phone on mobile data, not from the LAN." );
		if( c.PlexListening == true && !c.PlexMappedByUpnp )
			Out.Info( "Why Plex can work while your app's UPnP discovery fails: Plex may be using a manual forward, IPv6 or Relay (none needs UPnP), or it may discover the router in a way your code doesn't (every interface, unicast). The sections above show which of these the evidence supports." );
	}
}

static class MappingTest {

	public static async Task Run( int port, Context c, Options o, HttpClient http, CancellationToken ct ) {
		Out.Section( $"Temporary port-mapping test for TCP {port} (this changes the router, then undoes it)" );
		if( c.Primary is null ) { Fail( c, "no gateway", "no gateway known" ); return; }
		var cleanups = new List<(string What, Func<Task<string?>> Undo)>();
		TcpListener? listener = null;
		Task? accepting = null;
		using var stop = CancellationTokenSource.CreateLinkedTokenSource( ct );
		try {
			try { listener = new TcpListener( IPAddress.Any, port ); listener.Start(); }
			catch( SocketException ex ) { Fail( c, "listener", $"could not listen on TCP {port}: {ex.Message}" ); return; }
			Out.Info( $"listening on 0.0.0.0:{port} (answers each connection with one line of text; Windows may ask to allow it through the firewall)" );
			accepting = AcceptLoop( listener, stop.Token );

			string? mapped = await TryUpnp( port, c, o, http, cleanups, ct ) ?? await TryPcp( port, c, o, cleanups, ct ) ?? await TryNatPmp( port, c, o, cleanups, ct );
			if( mapped is null ) { Fail( c, "all", "no protocol would create a mapping (see the lines above)" ); return; }

			c.MappingTest = Status.Supported;
			c.MappingTestDetail = mapped;
			Out.Row( Status.Supported, "mapping created: " + mapped );

			if( o.Verify && int.TryParse( Regex.Match( mapped, @"external port (\d+)" ).Groups[1].Value, out int externalPort ) ) {
				try {
					string json = await http.GetStringAsync( $"https://ifconfig.co/port/{externalPort}", ct );
					using JsonDocument doc = JsonDocument.Parse( json );
					if( doc.RootElement.TryGetProperty( "reachable", out JsonElement reachable ) ) {
						Out.Row( reachable.GetBoolean() ? Status.Supported : Status.Failed, reachable.GetBoolean()
							? $"ifconfig.co reached port {externalPort} from the internet" : $"ifconfig.co could NOT reach port {externalPort}: the mapping exists but inbound doesn't arrive (double NAT, router firewall, or the router ignores its own mapping)" );
						c.MappingTest = reachable.GetBoolean() ? Status.Supported : Status.Failed;
						c.MappingTestDetail = mapped + ( reachable.GetBoolean() ? "; reachable from the internet" : "; NOT reachable from the internet" );
					}
					else Out.Row( Status.Inconclusive, $"unexpected answer from ifconfig.co: {Net.Trunc( json )}" );
				}
				catch( Exception ex ) when( ex is not OperationCanceledException ) { Out.Row( Status.Inconclusive, $"the external check failed: {ex.GetType().Name} {ex.Message}" ); }
			}
			else if( !o.Verify ) Out.Info( "Not verified from outside (use --verify, or try from a phone on mobile data). A connection from this LAN doesn't prove internet reachability." );

			if( o.Hold > 0 ) {
				Out.Info( $"holding the mapping for {o.Hold}s so you can test from outside; Ctrl+C ends it early..." );
				try { await Task.Delay( TimeSpan.FromSeconds( o.Hold ), ct ); } catch( OperationCanceledException ) { }
			}
		}
		catch( OperationCanceledException ) { Out.Info( "cancelled during the test" ); }
		finally {
			// Undo only what this run created, even after Ctrl+C.
			foreach( var (what, undo) in cleanups ) {
				try { string? problem = await undo(); Out.Row( problem is null ? Status.Supported : Status.Failed, problem is null ? $"removed {what}" : $"could not remove {what}: {problem} (it expires with its lease)" ); }
				catch( Exception ex ) { Out.Row( Status.Failed, $"could not remove {what}: {ex.Message} (it expires with its lease)" ); }
			}
			stop.Cancel();
			listener?.Stop();
			if( accepting is not null ) try { await accepting; } catch { }
		}
	}

	static void Fail( Context c, string what, string detail ) {
		c.MappingTest = Status.Failed; c.MappingTestDetail = detail;
		Out.Row( Status.Failed, $"{what}: {detail}" );
	}

	static async Task AcceptLoop( TcpListener listener, CancellationToken ct ) {
		byte[] reply = Encoding.ASCII.GetBytes( "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\nConnection: close\r\n\r\nRouterDiagnostics mapping test: connected\r\n" );
		while( !ct.IsCancellationRequested ) {
			try {
				using TcpClient client = await listener.AcceptTcpClientAsync( ct );
				await client.GetStream().WriteAsync( reply, ct );
			}
			catch { if( ct.IsCancellationRequested ) return; }
		}
	}

	// ---- UPnP: never overwrites; only uses an external port that is verifiably free ----
	static async Task<string?> TryUpnp( int port, Context c, Options o, HttpClient http, List<(string, Func<Task<string?>>)> cleanups, CancellationToken ct ) {
		foreach( IgdDevice d in c.Igds ) foreach( Service s in d.Connections ) {
			string who = $"UPnP {d.Friendly} {s.Type.Split( ':' )[^2]}";
			for( int external = port; external < port + 10; ++external ) {
				var (existing, error) = await UpnpClient.Soap( http, s, "GetSpecificPortMappingEntry", UpnpClient.PortArgs( external ), ct );
				bool free = existing is null && ( error ?? "" ).StartsWith( "714" );
				if( existing is not null ) { Out.Info( $"{who}: TCP {external} is already mapped (to {existing.GetValueOrDefault( "NewInternalClient" )}); leaving it alone and trying the next port" ); continue; }
				if( !free && !( c.UpnpMappingTableReadable && !c.Mappings.Any( m => m.External == external && m.Protocol == "TCP" ) ) ) {
					Out.Row( Status.Inconclusive, $"{who}: can't tell whether TCP {external} is free ({error}), so not adding" );
					break;
				}

				(string, string)[] args = [.. UpnpClient.PortArgs( external ), ("NewInternalPort", port.ToString()), ("NewInternalClient", d.Local.ToString()),
					("NewEnabled", "1"), ("NewPortMappingDescription", "RouterDiagnostics temp"), ("NewLeaseDuration", o.Lease.ToString())];
				var (added, addError) = await UpnpClient.Soap( http, s, "AddPortMapping", args, ct );
				if( added is null && ( addError ?? "" ).StartsWith( "725" ) ) { // OnlyPermanentLeasesSupported: we remove it ourselves at the end
					Out.Info( $"{who}: the router only supports permanent leases; adding one and removing it at the end" );
					args[^1] = ("NewLeaseDuration", "0");
					( added, addError ) = await UpnpClient.Soap( http, s, "AddPortMapping", args, ct );
				}
				if( added is null ) {
					Out.Row( addError!.StartsWith( "718" ) ? Status.Inconclusive : Status.Failed, $"{who}: AddPortMapping external {external} refused: {addError}" );
					if( addError.StartsWith( "718" ) ) continue; // someone else's mapping got there first
					break;
				}
				int ext = external; Service svc = s;
				cleanups.Add( ($"UPnP mapping TCP {ext}", async () => ( await UpnpClient.Soap( http, svc, "DeletePortMapping", UpnpClient.PortArgs( ext ), CancellationToken.None ) ).Error ));
				var (check, _) = await UpnpClient.Soap( http, s, "GetSpecificPortMappingEntry", UpnpClient.PortArgs( external ), ct );
				var (extIp, _) = await UpnpClient.Soap( http, s, "GetExternalIPAddress", [], ct );
				return $"{who}: external port {external} -> {d.Local}:{port}, lease {args[^1].Item2}s, router reports {( check is null ? "no entry on read-back (!)" : $"{check.GetValueOrDefault( "NewInternalClient" )}:{check.GetValueOrDefault( "NewInternalPort" )}" )}, external IP {extIp?.GetValueOrDefault( "NewExternalIPAddress" )}";
			}
		}
		return null;
	}

	// ---- PCP MAP ----
	static async Task<string?> TryPcp( int port, Context c, Options o, List<(string, Func<Task<string?>>)> cleanups, CancellationToken ct ) {
		foreach( GatewayInfo g in c.Gateways.Where( g => c.Pcp.GetValueOrDefault( g.Address ) == Status.Supported ) ) {
			byte[] nonce = new byte[12];
			Random.Shared.NextBytes( nonce );
			byte[] Request( int lifetime, int suggested ) {
				byte[] r = new byte[60];
				r[0] = 2; r[1] = 1; // version 2, MAP
				BitConverter.GetBytes( System.Net.IPAddress.HostToNetworkOrder( lifetime ) ).CopyTo( r, 4 );
				g.Local.MapToIPv6().GetAddressBytes().CopyTo( r, 8 );
				nonce.CopyTo( r, 24 );
				r[36] = 6; // TCP
				r[40] = (byte)( port >> 8 ); r[41] = (byte)port;
				r[42] = (byte)( suggested >> 8 ); r[43] = (byte)suggested;
				IPAddress.IPv6Any.GetAddressBytes().CopyTo( r, 44 );
				return r;
			}
			var (reply, error) = await Net.Exchange( g.Local, g.Address, 5351, Request( o.Lease, port ), ct );
			if( reply is null || reply.Length < 60 || reply[0] != 2 || reply[1] != 0x81 ) { Out.Row( Status.Failed, $"PCP MAP on {g.Address}: {error ?? ( reply is null ? "no reply" : "malformed reply " + Convert.ToHexString( reply ) )}" ); continue; }
			if( reply[3] != 0 ) { Out.Row( Status.Failed, $"PCP MAP on {g.Address}: result code {reply[3]}" ); continue; }
			int assigned = reply[42] << 8 | reply[43];
			var externalIp = new IPAddress( reply[44..60] );
			int lifetime = System.Net.IPAddress.NetworkToHostOrder( BitConverter.ToInt32( reply, 4 ) );
			GatewayInfo gw = g;
			cleanups.Add( ($"PCP mapping TCP {assigned}", async () => {
				var (r, e) = await Net.Exchange( gw.Local, gw.Address, 5351, Request( 0, assigned ), CancellationToken.None );
				return r is { Length: >= 60 } && r[3] == 0 ? null : e ?? ( r is null ? "no reply" : $"result code {r[3]}" );
			} ));
			return $"PCP {g.Address}: external port {assigned} -> {g.Local}:{port}, external IP {( externalIp.IsIPv4MappedToIPv6 ? externalIp.MapToIPv4() : externalIp )}, lifetime {lifetime}s";
		}
		return null;
	}

	// ---- NAT-PMP mapping: the router picks another external port if ours is taken ----
	static async Task<string?> TryNatPmp( int port, Context c, Options o, List<(string, Func<Task<string?>>)> cleanups, CancellationToken ct ) {
		foreach( GatewayInfo g in c.Gateways.Where( g => c.NatPmp.GetValueOrDefault( g.Address ) == Status.Supported ) ) {
			byte[] Request( int lifetime, int suggested ) => [0, 2, 0, 0, (byte)( port >> 8 ), (byte)port, (byte)( suggested >> 8 ), (byte)suggested,
				(byte)( lifetime >> 24 ), (byte)( lifetime >> 16 ), (byte)( lifetime >> 8 ), (byte)lifetime]; // opcode 2 = TCP
			var (reply, error) = await Net.Exchange( g.Local, g.Address, 5351, Request( o.Lease, port ), ct );
			if( reply is null || reply.Length < 16 || reply[1] != 130 ) { Out.Row( Status.Failed, $"NAT-PMP mapping on {g.Address}: {error ?? ( reply is null ? "no reply" : "malformed reply " + Convert.ToHexString( reply ) )}" ); continue; }
			int code = reply[2] << 8 | reply[3];
			if( code != 0 ) { Out.Row( Status.Failed, $"NAT-PMP mapping on {g.Address}: result code {code}" ); continue; }
			int assigned = reply[10] << 8 | reply[11];
			int lifetime = reply[12] << 24 | reply[13] << 16 | reply[14] << 8 | reply[15];
			GatewayInfo gw = g;
			cleanups.Add( ($"NAT-PMP mapping TCP {assigned}", async () => { // lifetime 0 and external 0 deletes the mapping for this internal port
				var (r, e) = await Net.Exchange( gw.Local, gw.Address, 5351, Request( 0, 0 ), CancellationToken.None );
				return r is { Length: >= 16 } && ( r[2] << 8 | r[3] ) == 0 ? null : e ?? ( r is null ? "no reply" : $"result code {( r[2] << 8 | r[3] )}" );
			} ));
			return $"NAT-PMP {g.Address}: external port {assigned} -> {g.Local}:{port}, lifetime {lifetime}s";
		}
		return null;
	}
}
