using System.Net;
using System.Net.Http.Json;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary> The API needs the shared access token in a header; the page itself does not. </summary>
public class AccessToken_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	HttpClient ClientWithToken( string? token ) {
		HttpClient client = factory.CreateClient(); // carries the right token by default
		client.DefaultRequestHeaders.Remove( AccessToken.HeaderName );
		if( token is not null ) client.DefaultRequestHeaders.Add( AccessToken.HeaderName, token );
		return client;
	}

	[Fact]
	public async Task TheApi_WithTheRightToken_Answers() {
		HttpResponseMessage response = await ClientWithToken( AccessToken.Value ).GetAsync( "/api/options" );

		response.StatusCode.ShouldBe( HttpStatusCode.OK );
	}

	[Fact]
	public async Task TheApi_WithoutAToken_Is401WithTheSameErrorBodyAsTheRest() {
		HttpResponseMessage response = await ClientWithToken( null ).GetAsync( "/api/options" );

		response.StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
		( await response.Content.ReadFromJsonAsync<ErrorView>() )!.Error.ShouldContain( "access token" );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "wrong" )]
	[InlineData( "blue3" )]   // a prefix of the token
	[InlineData( "blue322" )] // the token and more
	[InlineData( "BLUE32" )]  // not the same case
	public async Task TheApi_WithAWrongToken_Is401( string token ) {
		HttpResponseMessage response = await ClientWithToken( token ).GetAsync( "/api/options" );

		response.StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
	}

	[Fact]
	public async Task EveryApiCall_IsChecked_NotJustTheFirst() {
		HttpClient noToken = ClientWithToken( null );

		( await noToken.GetAsync( "/api/games" ) ).StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
		( await noToken.PostAsJsonAsync( "/api/games", new SpiritIsland.Web.Oracle.NewGameRequest( "River Surges in Sunlight", "A", 1 ) ) ).StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
		( await noToken.PostAsJsonAsync( "/api/games/abc/decide", new DecideRequest( 1, [] ) ) ).StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
		( await noToken.GetAsync( "/api/games/abc/static" ) ).StatusCode.ShouldBe( HttpStatusCode.Unauthorized );
	}

	[Fact]
	public async Task ThePage_NeedsNoToken_SoTheClientCanAskForOne() {
		HttpResponseMessage response = await ClientWithToken( null ).GetAsync( "/" );

		response.StatusCode.ShouldBe( HttpStatusCode.OK );
	}

	[Fact]
	public void TheTokenStartsAsBlue32() => AccessToken.Value.ShouldBe( "blue32" );
}
