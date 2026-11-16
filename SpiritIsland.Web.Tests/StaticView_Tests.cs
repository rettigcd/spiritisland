using System.Net;
using System.Net.Http.Json;
using SpiritIsland.Basegame;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// Phase 2 of docs/WebClient-Plan.md: the static view (<c>GET /api/games/{id}/static</c>): geometry, setup and the spirits'
/// rules, plus the version every response carries so a client knows when to fetch it again.
/// </summary>
public class StaticView_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	static readonly NewGameRequest River = new( RiverSurges.Name, "A", Seed: 1 );

	HttpClient Http => factory.CreateClient();

	async Task<OracleResponse> NewGameAsync( NewGameRequest request ) {
		HttpResponseMessage response = await Http.PostAsJsonAsync( "/api/games", request );
		response.EnsureSuccessStatusCode();
		return ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
	}

	async Task<StaticView> StaticOfAsync( string gameId )
		=> ( await Http.GetFromJsonAsync<StaticView>( $"/api/games/{gameId}/static" ) )!;

	#region geometry

	[Fact]
	public async Task Spaces_AreTheOnesTheViewDraws_EachWithAnOutline() {
		OracleResponse game = await NewGameAsync( River );

		StaticView view = await StaticOfAsync( game.GameId );

		view.Spaces.Select( s => s.Id ).ShouldBe( game.View.Spaces.Select( s => s.Id ), ignoreOrder: true );
		view.Spaces.Select( s => s.Id ).ShouldBeUnique();
		view.Spaces.ShouldAllBe( s => s.Corners.Length >= 3 );
		view.Spaces.ShouldAllBe( s => s.Terrain != "" );
	}

	[Fact]
	public async Task EverySpace_FitsInTheWorldBounds_WithItsNameAndTokensInside() {
		StaticView view = await StaticOfAsync( ( await NewGameAsync( River ) ).GameId );
		BoundsView world = view.WorldBounds;
		world.Width.ShouldBeGreaterThan( 0 );
		world.Height.ShouldBeGreaterThan( 0 );
		const double tolerance = 1e-4;

		foreach( SpaceGeometry space in view.Spaces ) {
			foreach( PointView corner in space.Corners ) {
				corner.X.ShouldBeInRange( world.X - tolerance, world.X + world.Width + tolerance, space.Id );
				corner.Y.ShouldBeInRange( world.Y - tolerance, world.Y + world.Height + tolerance, space.Id );
			}
			Polygons.IsInside( [.. space.Corners.Select( c => new XY( (float)c.X, (float)c.Y ) )], (float)space.NameLocation.X, (float)space.NameLocation.Y )
				.ShouldBeTrue( $"the name of {space.Id} is outside it" );
			space.TokenSize.ShouldBeGreaterThan( 0 );
			space.TokenSize.ShouldBeLessThan( world.Width );
		}
	}

	[Fact]
	public void TheIsland_IsTheSameWayUpAsTheDesktopAppsDrawIt() {
		// The engine's y points up and a screen's points down. The desktop apps turn the island over with PointMapper
		// (see FromWorldToViewport); the view's positions must be where PointMapper puts them, so a client draws them as they are.
		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( River ) );
		ActionScope.Initialize( gs.RootScope );
		StaticView view = StaticViewBuilder.Build( gs );
		Bounds world = new BoundsBuilder( gs.SpaceSpecs_Unfiltered.Select( s => s.Layout.Bounds ) ).GetBounds();
		PointMapper desktop = PointMapper.FromWorldToViewport( world, world ); // a viewport the size of the world: no scaling or moving, only the flip

		Dictionary<string, SpaceLayout> engine = gs.Spaces_Unfiltered.ToDictionary( s => s.SpaceSpec.Label, s => s.SpaceSpec.Layout );
		foreach( SpaceGeometry space in view.Spaces ) {
			SpaceLayout layout = engine[space.Id];
			space.Corners.Length.ShouldBe( layout.Corners.Length );
			for( int i = 0; i < layout.Corners.Length; ++i ) {
				XY expected = desktop.Map( layout.Corners[i] );
				space.Corners[i].X.ShouldBe( expected.X, 1e-3, $"{space.Id} corner {i}" );
				space.Corners[i].Y.ShouldBe( expected.Y, 1e-3, $"{space.Id} corner {i}" );
			}
		}
	}

	[Fact]
	public async Task Adjacency_IsMutual_AndNamesKnownSpaces() {
		StaticView view = await StaticOfAsync( ( await NewGameAsync( River ) ).GameId );
		Dictionary<string, SpaceGeometry> byId = view.Spaces.ToDictionary( s => s.Id );

		foreach( SpaceGeometry space in view.Spaces )
			foreach( string neighbour in space.Adjacent ) {
				byId.ShouldContainKey( neighbour, $"{space.Id} is next to {neighbour}, which isn't a space" );
				byId[neighbour].Adjacent.ShouldContain( space.Id, $"{neighbour} doesn't list {space.Id} as adjacent, but {space.Id} lists {neighbour}" );
			}
		view.Spaces.Count( s => s.Adjacent.Length > 0 ).ShouldBeGreaterThan( 0 );
	}

	#endregion geometry

	#region setup and spirits

	[Fact]
	public async Task Setup_NamesTheBoardsAndAnyAdversary() {
		StaticView plain = await StaticOfAsync( ( await NewGameAsync( River ) ).GameId );
		StaticView withAdversary = await StaticOfAsync( ( await NewGameAsync( new( RiverSurges.Name, "A", 1, "England", 2 ) ) ).GameId );

		plain.Setup.Boards.ShouldBe( ["A"] );
		plain.Setup.Adversary.ShouldBeNull();
		withAdversary.Setup.Adversary.ShouldNotBeNull();
		withAdversary.Setup.Adversary.Name.ShouldBe( "England" );
		withAdversary.Setup.Adversary.Level.ShouldBe( 2 );
		withAdversary.Setup.Adversary.Rules.ShouldNotBeNullOrEmpty();
	}

	[Fact]
	public async Task Spirit_ListsItsTracksGrowthAndInnates() {
		GameState reference = GameSetup.Builder.BuildGame( GameSetup.Configure( River ) );
		ActionScope.Initialize( reference.RootScope );
		Spirit spirit = reference.Spirits.Single();

		SpiritStatic described = ( await StaticOfAsync( ( await NewGameAsync( River ) ).GameId ) ).Spirits.Single();

		described.Id.ShouldBe( spirit.SpiritName );
		described.EnergyTrack.Length.ShouldBe( spirit.Presence.Energy.Slots.Count );
		described.CardPlayTrack.Length.ShouldBe( spirit.Presence.CardPlays.Slots.Count );
		described.EnergyTrack.Select( s => s.Id ).ShouldBe( spirit.Presence.Energy.Slots.Select( s => s.Code ) );
		described.EnergyTrack.ShouldContain( s => s.Energy != null, "an energy track with no energy on it" );
		described.GrowthGroups.Length.ShouldBe( spirit.GrowthTrack.Groups.Count() );
		described.GrowthGroups.ShouldAllBe( g => g.Actions.Length > 0 );
		described.Innates.Select( i => i.Name ).ShouldBe( spirit.InnatePowers.Select( p => p.Title ) );
		described.Innates.ShouldAllBe( i => i.Tiers.Length > 0 && i.Tiers.All( t => t.Threshold != "" ) );
	}

	[Fact]
	public async Task LiveViewOfTheSpirit_FitsTheStaticOne_AtEveryStepOfAGame() {
		OracleResponse game = await NewGameAsync( River );
		StaticView view = await StaticOfAsync( game.GameId );
		SpiritStatic described = view.Spirits.Single();

		for( int step = 0; step < 2_000 && game.Decision is not null; ++step ) {
			HttpResponseMessage response = await Http.PostAsJsonAsync( $"/api/games/{game.GameId}/decide",
				new DecideRequest( game.CheckpointVersion, [.. game.Picks, game.Decision.Options[0].Id] ) );
			response.EnsureSuccessStatusCode();
			game = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;

			SpiritView live = game.View.Spirits.Single();
			live.Presence.Energy.Revealed.ShouldAllBe( i => i < described.EnergyTrack.Length, $"step {step}" );
			live.Presence.CardPlays.Revealed.ShouldAllBe( i => i < described.CardPlayTrack.Length, $"step {step}" );
			live.GrowthGroupsUsed.ShouldAllBe( g => described.GrowthGroups.Any( s => s.Id == g ), $"step {step}" );
			live.Innates.Select( i => i.TierStatus.Length ).ShouldBe( described.Innates.Select( i => i.Tiers.Length ), $"step {step}" );
			game.View.Spaces.ShouldAllBe( s => view.Spaces.Any( g => g.Id == s.Id ), $"step {step}" );
		}
		game.Result.ShouldNotBeNull();
	}

	#endregion setup and spirits

	#region version

	[Fact]
	public async Task Version_IsTheSameInEveryResponse_AndInTheStaticView() {
		OracleResponse game = await NewGameAsync( River );
		string version = ( await StaticOfAsync( game.GameId ) ).StaticVersion;
		version.ShouldNotBeNullOrEmpty();
		game.StaticVersion.ShouldBe( version );

		for( int step = 0; step < 2_000 && game.Decision is not null; ++step ) {
			HttpResponseMessage response = await Http.PostAsJsonAsync( $"/api/games/{game.GameId}/decide",
				new DecideRequest( game.CheckpointVersion, [.. game.Picks, game.Decision.Options[0].Id] ) );
			game = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
			game.StaticVersion.ShouldBe( version, $"it changed at step {step}, though nothing about the island did" );
		}
		( await StaticOfAsync( game.GameId ) ).StaticVersion.ShouldBe( version );
	}

	[Fact]
	public async Task Version_DiffersWhenTheSetupDiffers() {
		string river = ( await NewGameAsync( River ) ).StaticVersion;
		string sameAgain = ( await NewGameAsync( River ) ).StaticVersion;
		string otherSpirit = ( await NewGameAsync( new( LightningsSwiftStrike.Name, "A", 1 ) ) ).StaticVersion;
		string otherBoard = ( await NewGameAsync( new( RiverSurges.Name, "B", 1 ) ) ).StaticVersion;
		string adversary = ( await NewGameAsync( new( RiverSurges.Name, "A", 1, "England", 2 ) ) ).StaticVersion;

		sameAgain.ShouldBe( river );
		new[] { river, otherSpirit, otherBoard, adversary }.ShouldBeUnique();
	}

	[Fact]
	public async Task StaticView_OfAGameThatIsOver_CanStillBeRead() {
		OracleResponse game = await NewGameAsync( River );
		for( int step = 0; step < 2_000 && game.Decision is not null; ++step ) {
			HttpResponseMessage response = await Http.PostAsJsonAsync( $"/api/games/{game.GameId}/decide",
				new DecideRequest( game.CheckpointVersion, [.. game.Picks, game.Decision.Options[0].Id] ) );
			game = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}
		game.Result.ShouldNotBeNull();

		( await StaticOfAsync( game.GameId ) ).StaticVersion.ShouldBe( game.StaticVersion );
	}

	[Fact]
	public async Task UnknownGame_Is404() {
		HttpResponseMessage response = await Http.GetAsync( "/api/games/nosuchgame/static" );

		response.StatusCode.ShouldBe( HttpStatusCode.NotFound );
	}

	#endregion version
}
