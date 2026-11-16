using SpiritIsland.Web.Endpoints;
using SpiritIsland.Web.Oracle;

var builder = WebApplication.CreateBuilder( args );

// Games are kept one JSON file each. GamesDir (setting or environment variable) overrides where.
builder.Services.AddSingleton<IGameFiles>( sp => new GameFiles(
	sp.GetRequiredService<IConfiguration>()["GamesDir"] is { Length: > 0 } dir ? dir : Path.Combine( AppContext.BaseDirectory, "games" ) ) );
builder.Services.AddSingleton<OracleService>();

WebApplication app = builder.Build();
app.MapGameEndpoints();
app.Run();

public partial class Program; // so the tests can host the app
