namespace SpiritIsland.Web.Network;

/// <summary>
/// The options the server takes on its command line, apart from ASP.NET's own settings:
///   --lan  let other devices on the local network connect (by default the server answers only this computer).
///   --wan  also ask the router (UPnP) to forward the port, so the server can be reached from the internet. It listens on the whole network
///          like --lan, so it includes it.
/// Run them after "--": dotnet run -- --lan. They are taken out of the arguments, because a bare "--lan" would otherwise swallow the next
/// argument as its value.
/// </summary>
/// <param name="Rest">The arguments that are left, for ASP.NET.</param>
public record ServerOptions( bool Lan, bool Wan, string[] Rest ) {

	public static ServerOptions FromArgs( string[] args ) => new(
		args.Contains( "--lan" ),
		args.Contains( "--wan" ),
		[.. args.Where( a => a != "--lan" && a != "--wan" )]
	);

	/// <summary> Whether the server listens on every network interface (not only this computer). </summary>
	public bool ListenOnNetwork => Lan || Wan;

	/// <summary> The port the server would have used: the first one in --urls / ASPNETCORE_URLS, or ASP.NET's own default of 5000. </summary>
	public static int PortOf( string? urls )
		=> ( urls ?? "http://localhost:5000" ).Split( ';' )
			.Select( u => Uri.TryCreate( u.Replace( "*", "localhost" ).Replace( "+", "localhost" ).Replace( "0.0.0.0", "localhost" ), UriKind.Absolute, out Uri? uri ) ? uri.Port : 5000 )
			.First();

}
