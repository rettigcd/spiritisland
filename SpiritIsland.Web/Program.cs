using Microsoft.AspNetCore.Diagnostics;
using SpiritIsland.Web;
using SpiritIsland.Web.Endpoints;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

// --lan: let other devices on the local network connect (by default the server answers only this computer). There is no login,
// so anyone who can reach this computer on that network can play and see the games. Run it as: dotnet run -- --lan
bool lan = args.Contains( "--lan" );
args = [.. args.Where( a => a != "--lan" )]; // not a setting, and a bare "--lan" would swallow the next argument as its value

var builder = WebApplication.CreateBuilder( args );
if( lan ) {
	// Keep the port that would have been used (--urls, ASPNETCORE_URLS, or ASP.NET's own default of 5000), on every network interface.
	string configured = builder.Configuration["urls"] ?? "http://localhost:5000";
	int port = configured.Split( ';' ).Select( u => Uri.TryCreate( u.Replace( "*", "localhost" ).Replace( "+", "localhost" ).Replace( "0.0.0.0", "localhost" ), UriKind.Absolute, out Uri? uri ) ? uri.Port : 5000 ).First();
	builder.WebHost.UseUrls( $"http://0.0.0.0:{port}" );
}

// Games are kept one JSON file each. GamesDir (setting or environment variable) overrides where.
builder.Services.AddSingleton<IGameFiles>( sp => new GameFiles(
	sp.GetRequiredService<IConfiguration>()["GamesDir"] is { Length: > 0 } dir ? dir : Path.Combine( AppContext.BaseDirectory, "games" ) ) );
builder.Services.AddSingleton<OracleService>();
// Numbers are numbers, never strings: this keeps the API description (and the client's generated types) to plain integers and doubles.
builder.Services.ConfigureHttpJsonOptions( o => o.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict );
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if( lan )
	app.Lifetime.ApplicationStarted.Register( () => {
		foreach( string address in LanAddresses() )
			app.Logger.LogInformation( "On your network, open http://{Address}:{Port}", address, new Uri( app.Urls.First().Replace( "0.0.0.0", "localhost" ) ).Port );
		app.Logger.LogWarning( "--lan: every device that can reach this computer can use the server. There is no login." );
	} );

// Anything unexpected (an engine failure, a bug) is a 500 with the same JSON error body as the rest of the API, so a client can show it.
app.UseExceptionHandler( errors => errors.Run( async context => {
	Exception? failure = context.Features.Get<IExceptionHandlerFeature>()?.Error;
	context.Response.StatusCode = StatusCodes.Status500InternalServerError;
	await context.Response.WriteAsJsonAsync( new ErrorView( failure?.Message ?? "Something went wrong." ) );
} ) );

// The API needs the access token (a header, so it can only be checked on the calls the client makes). The page itself, its scripts and
// pictures are served to anyone: they hold no game data, and they are what lets the client ask for the token.
app.Use( async ( context, next ) => {
	if( context.Request.Path.StartsWithSegments( "/api" ) && !AccessToken.IsValid( context.Request.Headers[AccessToken.HeaderName] ) ) {
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		await context.Response.WriteAsJsonAsync( new ErrorView( "A valid access token is required." ) );
		return;
	}
	await next();
} );

app.UseDefaultFiles(); // the built client (client/ builds into wwwroot)
app.UseStaticFiles();
app.MapGameEndpoints();
app.MapOpenApi();
/// <summary> This computer's IPv4 addresses on its running network connections (not loopback), which is what other devices type in. </summary>
static IEnumerable<string> LanAddresses() => System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
	.Where( n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
		&& n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback )
	.SelectMany( n => n.GetIPProperties().UnicastAddresses )
	.Where( a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
	.Select( a => a.Address.ToString() );

app.Run();

public partial class Program; // so the tests can host the app
