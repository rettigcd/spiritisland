using Microsoft.AspNetCore.Diagnostics;
using SpiritIsland.Web.Endpoints;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

var builder = WebApplication.CreateBuilder( args );

// Games are kept one JSON file each. GamesDir (setting or environment variable) overrides where.
builder.Services.AddSingleton<IGameFiles>( sp => new GameFiles(
	sp.GetRequiredService<IConfiguration>()["GamesDir"] is { Length: > 0 } dir ? dir : Path.Combine( AppContext.BaseDirectory, "games" ) ) );
builder.Services.AddSingleton<OracleService>();

WebApplication app = builder.Build();

// Anything unexpected (an engine failure, a bug) is a 500 with the same JSON error body as the rest of the API, so a client can show it.
app.UseExceptionHandler( errors => errors.Run( async context => {
	Exception? failure = context.Features.Get<IExceptionHandlerFeature>()?.Error;
	context.Response.StatusCode = StatusCodes.Status500InternalServerError;
	await context.Response.WriteAsJsonAsync( new ErrorView( failure?.Message ?? "Something went wrong." ) );
} ) );

app.MapGameEndpoints();
app.Run();

public partial class Program; // so the tests can host the app
