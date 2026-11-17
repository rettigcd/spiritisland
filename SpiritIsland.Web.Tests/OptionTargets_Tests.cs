using System.Net;
using System.Net.Http.Json;
using SpiritIsland.Basegame;
using SpiritIsland.Web.Oracle;
using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary>
/// Phase 2 of docs/WebClient-Plan.md: every option says what it points at, so the client can highlight it. The unit tests
/// build engine objects directly; the last one checks that, in real games, what options point at really exists in the view.
/// </summary>
public class OptionTargets_Tests( GamesFactory factory ) : IClassFixture<GamesFactory> {

	static (GameState Game, Spirit Spirit) NewGame() {
		GameState gs = GameSetup.Builder.BuildGame( GameSetup.Configure( new( RiverSurges.Name, "A", Seed: 1 ) ) );
		ActionScope.Initialize( gs.RootScope );
		return (gs, gs.Spirits.Single());
	}

	#region what each kind of option becomes

	[Fact]
	public void Space_PointsAtItsLabel() {
		var (gs, spirit) = NewGame();
		Space space = gs.Spaces_Existing.First();

		OptionTargets.For( space, spirit ).ShouldBe( new OptionTarget( "space", Space: space.SpaceSpec.Label ) );
	}

	[Fact]
	public void TokenOnASpace_PointsAtTheSpaceAndTheToken() {
		var (gs, spirit) = NewGame();
		Space space = gs.Spaces_Existing.First( s => s.SpaceSpec.Label.EndsWith( "4" ) );
		space.InitDefault( Human.Town, 1 );
		IToken town = space.OfType<IToken>().First( t => t.Text.StartsWith( "T" ) );

		OptionTargets.For( new SpaceToken( space, town ), spirit )
			.ShouldBe( new OptionTarget( "spaceToken", Space: space.SpaceSpec.Label, Token: town.Text ) );
	}

	[Fact]
	public void MoveOfATokenBetweenSpaces_PointsAtWhereItIsLeavingAndGoing() {
		var (gs, spirit) = NewGame();
		Space[] spaces = [.. gs.Spaces_Existing.Where( s => s.SpaceSpec.Label.StartsWith( "A" ) ).Take( 2 )];
		spaces[0].InitDefault( Human.Explorer, 1 );
		IToken explorer = spaces[0].OfType<IToken>().First( t => t.Text.StartsWith( "E" ) );

		OptionTargets.For( new Move( new SpaceToken( spaces[0], explorer ), spaces[1] ), spirit )
			.ShouldBe( new OptionTarget( "move", Space: spaces[0].SpaceSpec.Label, Token: explorer.Text, ToSpace: spaces[1].SpaceSpec.Label ) );
	}

	[Fact]
	public void MoveOfPresenceOffATrack_PointsAtTheSlotAndTheDestination() {
		var (gs, spirit) = NewGame();
		Track slot = spirit.Presence.Energy.Slots.Last();
		Space destination = gs.Spaces_Existing.First();

		OptionTarget target = OptionTargets.For( new Move( new TrackPresence( slot, spirit.Presence.Token ), destination ), spirit );

		target.Kind.ShouldBe( "move" );
		target.Spirit.ShouldBe( spirit.SpiritName );
		target.Track.ShouldBe( "energy" );
		target.Slot.ShouldBe( spirit.Presence.Energy.Slots.ToList().IndexOf( slot ) );
		target.Space.ShouldBeNull();
		target.ToSpace.ShouldBe( destination.SpaceSpec.Label );
	}

	[Fact]
	public void MoveOfDestroyedPresence_PointsAtTheSpiritAndTheDestination() {
		var (gs, spirit) = NewGame();
		Space destination = gs.Spaces_Existing.First();

		OptionTargets.For( new Move( new DestroyedPresence( spirit.Presence.Token ), destination ), spirit )
			.ShouldBe( new OptionTarget( "move", Spirit: spirit.SpiritName, ToSpace: destination.SpaceSpec.Label ) );
	}

	[Fact]
	public void TrackSlot_PointsAtWhichTrackAndWhichSlot() {
		var (_, spirit) = NewGame();
		Track energySlot = spirit.Presence.Energy.Slots.ElementAt( 1 );
		Track cardSlot = spirit.Presence.CardPlays.Slots.ElementAt( 2 );

		OptionTargets.For( energySlot, spirit ).ShouldBe( new OptionTarget( "track", Spirit: spirit.SpiritName, Track: "energy", Slot: 1 ) );
		OptionTargets.For( cardSlot, spirit ).ShouldBe( new OptionTarget( "track", Spirit: spirit.SpiritName, Track: "cardPlays", Slot: 2 ) );
	}

	[Fact]
	public void Card_PointsAtItsTitleAndTheSpirit() {
		var (_, spirit) = NewGame();
		PowerCard card = spirit.Hand.First();

		OptionTargets.For( card, spirit ).ShouldBe( new OptionTarget( "card", Spirit: spirit.SpiritName, Card: card.Title ) );
	}

	[Fact]
	public void Innate_PointsAtItsTitleAndTheSpirit() {
		var (_, spirit) = NewGame();
		InnatePower innate = spirit.InnatePowers.First();

		OptionTargets.For( innate, spirit ).ShouldBe( new OptionTarget( "innate", Spirit: spirit.SpiritName, Card: innate.Title ) );
	}

	[Fact]
	public void GrowthAction_PointsAtItsGroupAndItsPlaceInTheGroup() {
		var (_, spirit) = NewGame();
		GrowthGroup[] groups = spirit.GrowthTrack.Groups;
		int g = Array.FindIndex( groups, grp => 2 <= grp.GrowthActionFactories.Length );
		g.ShouldBeGreaterThanOrEqualTo( 0 ); // River has a group of more than one action

		OptionTargets.For( groups[g].GrowthActionFactories[1], spirit )
			.ShouldBe( new OptionTarget( "growth", Spirit: spirit.SpiritName, Group: g, Action: 1 ) );
	}

	[Fact]
	public void ACardTogetherWithALand_IsAnActPointingAtBoth() {
		var (gs, spirit) = NewGame();
		PowerCard card = spirit.Hand.First();
		Space land = gs.Spaces_Existing.First( s => s.SpaceSpec.Label.EndsWith( "4" ) );

		OptionTargets.For( new TargetedAction( card, land ), spirit )
			.ShouldBe( new OptionTarget( "act", Spirit: spirit.SpiritName, Card: card.Title, ToSpace: land.SpaceSpec.Label ) );
	}

	[Fact]
	public void AnInnateTogetherWithASpirit_IsAnActPointingAtBoth() {
		var (_, spirit) = NewGame();
		InnatePower innate = spirit.InnatePowers.First();

		OptionTargets.For( new TargetedAction( innate, spirit ), spirit )
			.ShouldBe( new OptionTarget( "act", Spirit: spirit.SpiritName, Card: innate.Title, ToSpirit: spirit.SpiritName ) );
	}

	[Fact]
	public void AGrowthActionTogetherWithTheSpirit_IsAnActWithItsGroupAndAction() {
		var (_, spirit) = NewGame();
		GrowthGroup[] groups = spirit.GrowthTrack.Groups;
		int g = Array.FindIndex( groups, grp => 2 <= grp.GrowthActionFactories.Length );

		OptionTargets.For( new TargetedAction( groups[g].GrowthActionFactories[1], spirit ), spirit )
			.ShouldBe( new OptionTarget( "act", Spirit: spirit.SpiritName, Group: g, Action: 1, ToSpirit: spirit.SpiritName ) );
	}

	[Fact]
	public void ChoicesWithNothingToPointAt_AreOther() {
		var (_, spirit) = NewGame();

		OptionTargets.For( new TextOption( "Done" ), spirit ).Kind.ShouldBe( "other" );
	}

	#endregion what each kind of option becomes

	#region in real games

	[Fact]
	public async Task WhatOptionsPointAt_ExistsInTheView_AcrossRealGames() {
		var http = factory.CreateClient();
		// Spirits whose games this engine plays to the end without failing; other spirits' engine bugs aren't this test's business.
		string[] spirits = [RiverSurges.Name, LightningsSwiftStrike.Name, Shadows.Name, ASpreadOfRampantGreen.Name, Bringer.Name];
		var kinds = new Dictionary<string, int>();
		int seed = 0;

		foreach( string spirit in spirits ) {
			GameState reference = GameSetup.Builder.BuildGame( GameSetup.Configure( new( spirit, "A", Seed: 1 ) ) );
			ActionScope.Initialize( reference.RootScope );
			Spirit referenceSpirit = reference.Spirits.Single();
			int energySlots = referenceSpirit.Presence.Energy.Slots.Count, cardSlots = referenceSpirit.Presence.CardPlays.Slots.Count;
			var rnd = new Random( ++seed );

			HttpResponseMessage created = await http.PostAsJsonAsync( "/api/games", new NewGameRequest( spirit, "A", 1 ) );
			OracleResponse last = ( await created.Content.ReadFromJsonAsync<OracleResponse>() )!;

			for( int step = 0; step < 250 && last.Decision is not null; ++step ) {
				foreach( OptionView option in last.Decision.Options ) {
					kinds[option.Target.Kind] = kinds.GetValueOrDefault( option.Target.Kind ) + 1;
					AssertPointsAtSomethingReal( option, last, energySlots, cardSlots, $"{spirit}, step {step}, '{last.Decision.Prompt}'" );
				}
				string pick = last.Decision.Options[rnd.Next( last.Decision.Options.Length )].Id;
				HttpResponseMessage response = await http.PostAsJsonAsync( $"/api/games/{last.GameId}/decide",
					new DecideRequest( last.CheckpointVersion, [.. last.Picks, pick] ) );
				if( response.StatusCode == HttpStatusCode.InternalServerError ) break; // an engine failure, covered elsewhere
				response.EnsureSuccessStatusCode();
				last = ( await response.Content.ReadFromJsonAsync<OracleResponse>() )!;
			}
		}

		// so this can't pass by looking at nothing
		foreach( string kind in new[] { "space", "spaceToken", "move", "card", "act", "other" } )
			kinds.GetValueOrDefault( kind ).ShouldBeGreaterThan( 0, $"no '{kind}' option was ever offered" );
	}

	static void AssertPointsAtSomethingReal( OptionView option, OracleResponse response, int energySlots, int cardSlots, string where ) {
		GameView view = response.View;
		OptionTarget t = option.Target;
		SpaceView? Space( string? id ) => view.Spaces.SingleOrDefault( s => s.Id == id );
		void SpaceHasToken( string? spaceId, string? tokenId ) {
			Space( spaceId ).ShouldNotBeNull( $"{where}: '{option.Id}' points at space {spaceId}, which isn't in the view" );
			Space( spaceId )!.Tokens.ShouldContain( tok => tok.Id == tokenId, $"{where}: '{option.Id}' points at {tokenId} on {spaceId}, which isn't drawn there" );
		}
		void TrackSlotExists() {
			t.Spirit.ShouldBe( view.ViewerSpiritId, $"{where}: '{option.Id}' is {t}" );
			t.Slot.ShouldNotBeNull( $"{where}: '{option.Id}' is {t}" );
			( t.Track == "energy" ? energySlots : cardSlots ).ShouldBeGreaterThan( t.Slot!.Value, $"{where}: '{option.Id}' points past the end of the {t.Track} track" );
		}

		switch( t.Kind ) {
			case "space":
				Space( t.Space ).ShouldNotBeNull( $"{where}: '{option.Id}' points at space {t.Space}, which isn't in the view" );
				break;
			case "spaceToken":
				SpaceHasToken( t.Space, t.Token );
				break;
			case "move":
				if( t.Space is not null ) SpaceHasToken( t.Space, t.Token );
				else if( t.Track is not null ) TrackSlotExists();
				else t.Spirit.ShouldBe( view.ViewerSpiritId, $"{where}: '{option.Id}' is {t}" ); // from the destroyed pile
				if( t.ToSpace is not null ) Space( t.ToSpace ).ShouldNotBeNull( $"{where}: '{option.Id}' goes to {t.ToSpace}, which isn't in the view" );
				break;
			case "track" or "trackPresence":
				TrackSlotExists();
				break;
			case "card":
				t.Spirit.ShouldBe( view.ViewerSpiritId, where );
				t.Card.ShouldNotBeNullOrEmpty( where );
				// cards being resolved or played are the spirit's own; cards on offer from a deck aren't in the view yet
				SpiritView mine = view.Spirits.Single( s => s.Id == t.Spirit );
				if( response.Decision!.Prompt.StartsWith( "Select Fast to resolve" ) || response.Decision.Prompt.StartsWith( "Select Slow to resolve" ) )
					mine.InPlay.ShouldContain( c => c.Id == t.Card, $"{where}: '{t.Card}' isn't in play" );
				if( response.Decision.Prompt.StartsWith( "Play power card" ) )
					mine.Hand.ShouldContain( c => c.Id == t.Card, $"{where}: '{t.Card}' isn't in the hand" );
				break;
			case "innate":
				view.Spirits.Single( s => s.Id == t.Spirit ).Innates.ShouldContain( i => i.Name == t.Card, $"{where}: innate '{t.Card}' isn't the spirit's" );
				break;
			case "growth":
				t.Spirit.ShouldBe( view.ViewerSpiritId, where );
				t.Group.ShouldNotBeNull( where );
				t.Action.ShouldNotBeNull( where );
				break;
			case "act":
				t.Spirit.ShouldBe( view.ViewerSpiritId, where );
				( t.Card is not null || t.Group is not null ).ShouldBeTrue( $"{where}: '{option.Id}' is an act of nothing" );
				( t.ToSpace is not null || t.ToSpirit is not null ).ShouldBeTrue( $"{where}: '{option.Id}' is an act with no target" );
				if( t.ToSpace is not null ) Space( t.ToSpace ).ShouldNotBeNull( $"{where}: '{option.Id}' targets {t.ToSpace}, which isn't in the view" );
				if( t.ToSpirit is not null ) t.ToSpirit.ShouldBe( view.ViewerSpiritId, $"{where}: '{option.Id}' targets another spirit" );
				break;
			case "action" or "other" or "destroyedPresence":
				break;
			default:
				throw new Exception( $"{where}: unknown option kind '{t.Kind}'" );
		}
	}

	#endregion in real games
}
