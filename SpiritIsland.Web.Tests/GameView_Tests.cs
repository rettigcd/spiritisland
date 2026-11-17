using System.Net.Http.Json;
using System.Text.Json;
using SpiritIsland.Basegame;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// Phase 2 of docs/WebClient-Plan.md: the player view. What it must never show (hidden information), and that every token
/// is drawn inside its own space. Most tests build a game directly so they can compare the view with the engine's own
/// state; one plays a game over HTTP. Games are only known to be safe in separate execution contexts, so these run one
/// at a time.
/// </summary>
public class GameView_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	static readonly NewGameRequest River = new( RiverSurges.Name, "A", Seed: 1 );

	static (GameState Game, GameView View) NewGame( NewGameRequest request ) {
		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( request ) );
		return (gs, GameViewBuilder.Build( gs, gs.Spirits.Single() ));
	}

	static string Json( object value ) => JsonSerializer.Serialize( value );

	#region the spirit

	[Fact]
	public void Spirit_IsDescribedFromItsOwnState() {
		var (gs, view) = NewGame( River );
		Spirit spirit = gs.Spirits.Single();

		view.ViewerSpiritId.ShouldBe( spirit.SpiritName );
		SpiritView described = view.Spirits.Single();
		described.Id.ShouldBe( spirit.SpiritName );
		described.Energy.ShouldBe( spirit.Energy );
		described.Hand.Select( c => c.Id ).ShouldBe( spirit.Hand.Select( c => c.Title ) );
		described.Hand.ShouldAllBe( c => c.Name != "" && c.Speed != "" );
		described.InPlay.ShouldBeEmpty();
		described.Discard.ShouldBeEmpty();
		described.Innates.Select( i => i.Name ).ShouldBe( spirit.InnatePowers.Select( p => p.Title ) );
		described.Innates.ShouldAllBe( i => i.TierStatus.Length > 0 );
		described.Presence.Energy.Revealed.Length.ShouldBe( spirit.Presence.Energy.Revealed.Count() );
		described.Presence.CardPlays.Revealed.Length.ShouldBe( spirit.Presence.CardPlays.Revealed.Count() );
		described.Presence.EnergyPerTurn.ShouldBe( spirit.Presence.EnergyPerTurn );
	}

	[Fact]
	public void RevealedTrackSlots_AreIndexesIntoTheTrack() {
		var (gs, view) = NewGame( River );
		Spirit spirit = gs.Spirits.Single();

		int slotCount = spirit.Presence.Energy.Slots.Count;

		view.Spirits.Single().Presence.Energy.Revealed.ShouldAllBe( i => 0 <= i && i < slotCount );
		view.Spirits.Single().Presence.Energy.Revealed.ShouldBeUnique();
	}

	#endregion the spirit

	#region hidden information

	[Fact]
	public void Fear_ShowsHowManyCardsRemainButNotWhichOnes() {
		var (gs, view) = NewGame( River );
		gs.Fear.Deck.Count.ShouldBeGreaterThan( 0 );

		view.Fear.FacedownCardsRemaining.ShouldBe( gs.Fear.Deck.Count );
		string json = Json( view );
		foreach( IFearCard card in gs.Fear.Deck )
			json.ShouldNotContain( ((IOption)card).Text, Case.Sensitive, "a face-down fear card's name leaked" );
	}

	[Fact]
	public void Blight_HidesTheCardUntilItFlips() {
		var (gs, view) = NewGame( River );
		gs.BlightCard.CardFlipped.ShouldBeFalse();

		view.Blight.CardFlipped.ShouldBeFalse();
		view.Blight.Card.ShouldBeNull();
		view.Blight.BlightedIslandEffect.ShouldBeNull();
		view.Blight.BlightRemainingOnCard.ShouldBe( (int)gs.BlightCard.Memento );
		Json( view ).ShouldNotContain( gs.BlightCard.Name, Case.Sensitive, "the unflipped blight card's name leaked" );
	}

	[Fact]
	public void Invaders_ShowTheSlotsButOnlyCountTheDeck() {
		var (gs, view) = NewGame( River );
		InvaderDeck deck = gs.InvaderDeck;

		// the card waiting in the Explore slot has not been flipped yet, so it is counted with the deck and not shown
		view.Invaders.UnrevealedCount.ShouldBe( deck.UnrevealedCards.Count + deck.Explore.Cards.Count );
		deck.Explore.Cards.ShouldNotBeEmpty( "the engine has drawn the next Explore card, which is what must stay hidden" );
		view.Invaders.Build.ShouldBe( deck.Build.Cards.Select( c => ((IOption)c).Text ) );
		view.Invaders.Ravage.ShouldBe( deck.Ravage.Cards.Select( c => ((IOption)c).Text ) );
		// nothing in the view is a list the length of the deck that could be its order
		typeof( InvadersView ).GetProperties().Where( p => p.PropertyType == typeof( string[] ) ).Select( p => p.Name )
			.ShouldBe( ["Build", "Ravage", "Discards"] );
	}

	[Fact]
	public async Task NothingHidden_AppearsAtAnyPointInAGame() {
		// Every response of a whole game: no face-down fear card or unflipped blight card names, ever.
		(GameState gs, _) = NewGame( River );
		string[] fearNames = [.. gs.Fear.Deck.Select( c => ((IOption)c).Text )];
		string blightName = gs.BlightCard.Name;

		var player = new HttpPlayer( factory );
		await player.NewAsync( River );
		for( int step = 0; step < 2_000 && player.Last.Decision is not null; ++step ) {
			await player.PickAsync( player.Last.Decision.Options[0].Id );
			GameView view = player.Last.View;
			string json = Json( view );
			// The view doesn't say which earned cards are face up, so only look while none has left the deck.
			if( view.Fear.FacedownCardsRemaining == fearNames.Length )
				foreach( string name in fearNames )
					json.ShouldNotContain( name, Case.Sensitive, $"a fear card's name leaked at step {step}" );
			if( !view.Blight.CardFlipped )
				json.ShouldNotContain( blightName, Case.Sensitive, $"the blight card's name leaked at step {step}" );
		}
	}

	#endregion hidden information

	[Theory]
	[InlineData( "G" )]
	[InlineData( "H" )]
	public void HorizonsBoards_AreOffered_AndBuildAGame( string board ) {
		GameSetup.Boards.ShouldContain( board );

		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( new( RiverSurges.Name, board, Seed: 1 ) ) );
		ActionScope.Initialize( gs.RootScope );
		GameView view = GameViewBuilder.Build( gs, gs.Spirits.Single() );

		view.Spaces.ShouldNotBeEmpty();
		view.Spaces.ShouldAllBe( s => s.Id.StartsWith( board ) || s.Id.StartsWith( "Ocean", StringComparison.OrdinalIgnoreCase ) );
	}

	#region tokens

	[Fact]
	public void Tokens_ShowTheirCountsAndAreDrawnInsideTheirSpace() {
		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( River ) );
		ActionScope.Initialize( gs.RootScope );
		Space crowded = gs.Spaces_Existing.First( s => s.SpaceSpec.Label.EndsWith( "5" ) );
		crowded.InitDefault( Human.Explorer, 3 );
		crowded.InitDefault( Human.Town, 2 );
		crowded.InitDefault( Human.City, 1 );
		crowded.InitDefault( Human.Dahan, 4 );
		crowded.Init( Token.Blight, 2 );
		crowded.Init( Token.Beast, 1 );
		crowded.Init( Token.Wilds, 1 );
		crowded.Init( Token.Disease, 1 );
		crowded.Init( Token.Defend, 5 );

		GameView view = GameViewBuilder.Build( gs, gs.Spirits.Single() );

		SpaceView drawn = view.Spaces.Single( s => s.Id == crowded.SpaceSpec.Label );
		foreach( IToken token in crowded.OfType<IToken>() )
			drawn.Tokens.Single( t => t.Id == token.Text ).Count.ShouldBe( crowded[token] );
		drawn.Tokens.Select( t => t.Id ).ShouldBeUnique();
		AssertInsideTheirSpaces( gs, view );
	}

	[Fact]
	public void Tokens_KeepTheirPlaces_WhenTheViewIsBuiltAgain() {
		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( River ) );
		ActionScope.Initialize( gs.RootScope );
		Space space = gs.Spaces_Existing.First( s => s.SpaceSpec.Label.EndsWith( "3" ) );
		space.InitDefault( Human.Town, 2 );
		space.InitDefault( Human.Dahan, 2 );
		space.Init( Token.Blight, 1 );

		Json( GameViewBuilder.Build( gs, gs.Spirits.Single() ) )
			.ShouldBe( Json( GameViewBuilder.Build( gs, gs.Spirits.Single() ) ) );
	}

	[Fact]
	public async Task Tokens_AreInsideTheirSpace_AtEveryStepOfAGame() {
		(GameState gs, _) = NewGame( River ); // the same board, so the same layout, as the game being played
		var player = new HttpPlayer( factory );
		await player.NewAsync( River );
		int tokensSeen = 0;

		for( int step = 0; step < 2_000 && player.Last.Decision is not null; ++step ) {
			await player.PickAsync( player.Last.Decision.Options[0].Id );
			AssertInsideTheirSpaces( gs, player.Last.View );
			player.Last.View.Spaces.Select( s => s.Id ).ShouldBeUnique();
			tokensSeen += player.Last.View.Spaces.Sum( s => s.Tokens.Length );
		}

		tokensSeen.ShouldBeGreaterThan( 0 );
	}

	static void AssertInsideTheirSpaces( GameState gs, GameView view ) {
		var screen = ScreenCoordinates.For( gs ); // the views' positions are the engine's turned over
		Dictionary<string, XY[]> outlines = gs.Spaces_Unfiltered.ToDictionary( s => s.SpaceSpec.Label,
			s => s.SpaceSpec.Layout.Corners.Select( c => new XY( c.X, (float)screen.Y( c.Y ) ) ).ToArray() );
		foreach( SpaceView space in view.Spaces )
			foreach( TokenView token in space.Tokens )
				Polygons.IsInside( outlines[space.Id], (float)token.X, (float)token.Y )
					.ShouldBeTrue( $"{token.Id} on {space.Id} at ({token.X}, {token.Y}) is outside the space" );
	}

	#endregion tokens

	/// <summary> What a browser holds: the last response, whose pending picks it adds to. </summary>
	sealed class HttpPlayer( GamesFactory host ) {
		readonly HttpClient _http = host.CreateClient();
		public OracleResponse Last { get; private set; } = null!;

		public async Task NewAsync( NewGameRequest request ) {
			HttpResponseMessage response = await _http.PostAsJsonAsync( "/api/games", request );
			response.EnsureSuccessStatusCode();
			Last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}

		public async Task PickAsync( string option ) {
			HttpResponseMessage response = await _http.PostAsJsonAsync( $"/api/games/{Last.GameId}/decide",
				new DecideRequest( Last.CheckpointVersion, [.. Last.Picks, option] ) );
			response.EnsureSuccessStatusCode();
			Last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
		}
	}
}
