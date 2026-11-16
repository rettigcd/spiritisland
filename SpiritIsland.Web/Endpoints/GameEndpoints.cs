using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Endpoints;

/// <summary> The HTTP layer: JSON in and out and status codes. Nothing about the game itself lives here. </summary>
public static class GameEndpoints {

	public static void MapGameEndpoints( this IEndpointRouteBuilder app ) {
		RouteGroupBuilder api = app.MapGroup( "/api" );

		api.MapGet( "/options", () => new SetupOptions( GameSetup.Spirits, GameSetup.Boards, GameSetup.Adversaries ) );

		api.MapGet( "/games", ( IGameFiles files ) => files.List() );

		api.MapPost( "/games", async ( NewGameRequest request, OracleService oracle ) => {
			try { return Results.Ok( await oracle.CreateAsync( request ) ); }
			catch( ArgumentException ex ) { return Results.BadRequest( new ErrorView( ex.Message ) ); }
		} );

		// picks: [] just reads the current decision
		api.MapGet( "/games/{id}/static", async ( string id, OracleService oracle )
			=> await oracle.StaticAsync( id ) is { } view ? Results.Ok( view ) : Results.NotFound( new ErrorView( $"No game '{id}'." ) ) );

		api.MapPost( "/games/{id}/decide", async ( string id, DecideRequest request, OracleService oracle )
			=> await oracle.DecideAsync( id, request ) switch {
				DecideOutcome.Ok ok => Results.Ok( ok.Response ),
				DecideOutcome.Stale stale => Results.Conflict( stale.Current ),
				DecideOutcome.BadPick bad => Results.BadRequest( new ErrorView( bad.Message ) ),
				_ => Results.NotFound( new ErrorView( $"No game '{id}'." ) ),
			} );
	}
}
