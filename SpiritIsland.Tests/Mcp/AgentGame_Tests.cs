#nullable enable
using SpiritIsland.Agent;

namespace SpiritIsland.Tests.Mcp;

/// <summary>
/// Headless agent-facing API. Games are NOT known to be safe to run concurrently, so every test plays its games
/// one at a time from inside this single class (xunit runs a class's tests serially).
/// </summary>
public class AgentGame_Tests {

	static NewGameRequest Req( int seed = 1, string? adversary = null, int level = 0 )
		=> new( RiverSurges.Name, "A", seed, adversary, level );

	[Fact]
	public void RandomAgent_PlaysToTerminalState() {
		var game = new AgentGame( Req() );
		var result = RandomAgent.Play( game, agentSeed: 1 );

		result.ShouldNotBeNull();
		game.IsOver.ShouldBeTrue();
		game.GetDecision().ShouldBeNull();
		game.Error.ShouldBeNull();
	}

	[Fact]
	public void RandomAgent_PlaysWithAdversary() {
		var game = new AgentGame( Req( 2, "England", 1 ) );
		RandomAgent.Play( game, agentSeed: 2 ).ShouldNotBeNull();
	}

	[Fact]
	public void StaleVersion_IsRejected_AndGameDoesNotAdvance() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;

		Should.Throw<ArgumentException>( () => game.Select( d.Version + 1, d.Options[0] ) );

		game.GetDecision()!.Version.ShouldBe( d.Version );
		game.History.ShouldBeEmpty();
		game.Select( d.Version, d.Options[0] ); // original still usable
		Should.Throw<ArgumentException>( () => game.Select( d.Version, d.Options[0] ) ); // now stale
	}

	[Fact]
	public void UnknownOption_IsRejected() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;

		Should.Throw<ArgumentException>( () => game.Select( d.Version, "not an option" ) );

		game.GetDecision()!.Version.ShouldBe( d.Version );
	}

	[Fact]
	public void OptionIds_AreUniqueWithinEachDecision() {
		var game = new AgentGame( Req() );
		var rnd = new Random( 3 );
		for( int i = 0; i < 40; ++i ) {
			DecisionSnapshot? d = game.GetDecision();
			if( d is null ) break;
			d.Options.Distinct().Count().ShouldBe( d.Options.Length, d.Prompt );
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
		}
	}

	[Fact]
	public void SameSeedAndChoices_ReproduceSameGame() {
		var a = new AgentGame( Req( 7 ) );
		var ra = RandomAgent.Play( a, 5 );
		var b = new AgentGame( Req( 7 ) );
		var rb = RandomAgent.Play( b, 5 );

		b.History.ShouldBe( a.History );
		rb!.ToString().ShouldBe( ra!.ToString() );
		b.GameState.RoundNumber.ShouldBe( a.GameState.RoundNumber );
	}

	[Fact]
	public void Observation_DoesNotRevealHiddenDecks() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		var gs = game.GameState;
		string json = PlayerObservation.Build( game ).ToJsonString();

		// Invader deck: only revealed slot cards and a count are present
		var invaders = PlayerObservation.Build( game )["invaders"]!.AsObject();
		invaders.Select( kv => kv.Key ).Order().ShouldBe( ["build", "discards", "explore", "matchedLands", "ravage", "unrevealedCount"] );
		invaders["unrevealedCount"]!.GetValue<int>().ShouldBe( gs.InvaderDeck.UnrevealedCards.Count );

		// Fear: only a count of face-down cards, no identities
		foreach( IFearCard hidden in gs.Fear.Deck )
			json.ShouldNotContain( ((IOption)hidden).Text );
		PlayerObservation.Build( game )["fear"]!["facedownCardsRemaining"]!.GetValue<int>().ShouldBe( gs.Fear.Deck.Count );
	}

	[Fact]
	public void Observation_ReportsSpiritAndIsland() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		var obs = PlayerObservation.Build( game );

		obs["spirit"]!["name"]!.GetValue<string>().ShouldBe( RiverSurges.Name );
		obs["spirit"]!["hand"]!.AsArray().Count.ShouldBeGreaterThan( 0 );
		obs["spaces"]!.AsArray().Count.ShouldBe( 9 ); // ocean + 8 lands
	}

	[Fact]
	public void PlayPrompt_SpellsOutEnergyAndCardPlays() {
		var game = new AgentGame( Req() );

		// Growth first, then the engine asks which card to play.
		DecisionSnapshot d = game.GetDecision()!;
		for( int i = 0; i < 10 && !d.Prompt.StartsWith( "Play power card" ); ++i ) {
			game.Select( d.Version, d.Options[0] );
			d = game.GetDecision()!;
		}

		d.Prompt.ShouldStartWith( "Play power card" ); // if the engine's wording changes, fix PromptDetails and this fails first
		Spirit spirit = game.GameState.Spirits.Single();
		d.PromptDetails.ShouldBe( $"Energy: {spirit.Energy}. Card plays left: {spirit.Presence.CardPlayPerTurn}." );
	}

	[Fact]
	public void PromptDetails_IsNullForPromptsWithNothingToSpellOut() {
		PromptDetails.For( "Select Growth" ).ShouldBeNull();
		PromptDetails.For( "Select Fast to resolve (Slow as Fast: 1 of 2 left)" ).ShouldBeNull();
		PromptDetails.For( "Play power card ($6 / 4)" ).ShouldBe( "Energy: 6. Card plays left: 4." );
	}

	[Fact]
	public void Setup_RejectsUnknownSpiritBoardAndAdversary() {
		Should.Throw<ArgumentException>( () => new AgentGame( new( "Nope", "A", 1 ) ) );
		Should.Throw<ArgumentException>( () => new AgentGame( new( RiverSurges.Name, "Z", 1 ) ) );
		Should.Throw<ArgumentException>( () => new AgentGame( Req( 1, "Atlantis", 1 ) ) );
		Should.Throw<ArgumentException>( () => new AgentGame( Req( 1, "England", 99 ) ) );
	}

	[Fact]
	public void GrowthOptions_AreLabelledWithTheirGroup() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;

		d.Prompt.ShouldBe( "Select Growth" );
		d.Options.ShouldAllBe( o => o.Contains( "(group: " ) || !o.Contains( " / " ) );
		d.Options.Count( o => o.Contains( "(group: " ) ).ShouldBeGreaterThan( 0 );
		d.Options.Distinct().Count().ShouldBe( d.Options.Length );

		var growth = PlayerObservation.Build( game )["spirit"]!["growth"]!.AsArray();
		growth.Count.ShouldBeGreaterThan( 0 );
	}
}
