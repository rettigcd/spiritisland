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
	public void SingleOptionDecisions_AreAutoSelected_AndReportedAsAlerts() {
		var game = new AgentGame( Req() );
		var rnd = new Random( 4 );
		var autoAlerts = new List<Alert>();
		for( int i = 0; i < 150; ++i ) {
			DecisionSnapshot? d = game.GetDecision();
			if( d is null ) break;
			d.Options.Length.ShouldBeGreaterThan( 1, d.Prompt );
			autoAlerts.AddRange( d.Alerts.Where( a => a.Kind == "auto" ) );
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
		}
		autoAlerts.ShouldNotBeEmpty(); // River's growth groups alone produce some
		autoAlerts.ShouldAllBe( a => a.Text.StartsWith( "Auto-selected '" ) );
	}

	[Fact]
	public void AutoSelect_PlaysTheSameGameAsChoosingTheOnlyOptionByHand() {
		// Always taking the first option: auto-select must make exactly the choices a player would have had to.
		static List<string> Play( bool auto ) {
			var game = new AgentGame( Req( 5 ) ) { AutoSelectSingleOption = auto };
			for( int i = 0; i < 150; ++i ) {
				DecisionSnapshot? d = game.GetDecision();
				if( d is null ) break;
				game.Select( d.Version, d.Options[0] );
			}
			game.GetDecision(); // settle any trailing auto-selections
			return game.History;
		}
		List<string> manual = Play( false );
		List<string> auto = Play( true );
		auto.Count.ShouldBeGreaterThanOrEqualTo( manual.Count ); // 150 real decisions plus the auto ones reach further
		auto.Take( manual.Count ).ShouldBe( manual );
	}

	[Fact]
	public void Batch_PlaysTheSameGameAsOneAtATime_IncludingStepsThatWereAutoSelected() {
		// One at a time, random choices. History also records the auto-selected choices.
		var a = new AgentGame( Req( 8 ) );
		var rnd = new Random( 8 );
		int autoCount = 0;
		for( int i = 0; i < 40; ++i ) {
			DecisionSnapshot? d = a.GetDecision();
			if( d is null ) break;
			autoCount += d.Alerts.Count( x => x.Kind == "auto" );
			a.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
		}
		autoCount.ShouldBeGreaterThan( 0 );

		// The same choices as one batch - including the ones the server picks by itself.
		var b = new AgentGame( Req( 8 ) );
		BatchResult r = b.SelectBatch( b.GetDecision()!.Version, [.. a.History.Select( o => new BatchStep( o ) )] );

		r.StoppedBecause.ShouldBeNull();
		r.Applied.ShouldBe( a.History.Count );
		b.History.ShouldBe( a.History );
		b.GetDecision()?.Prompt.ShouldBe( a.GetDecision()?.Prompt );
	}

	[Fact]
	public void Batch_ReportsEachStep_AndKeepsTheAlertsOfDecisionsItPassedThrough() {
		var game = new AgentGame( Req() );
		var steps = new List<BatchStep>();
		var a = new AgentGame( Req() ); // find a few valid steps, with their prompts
		var expectedAlerts = new List<string>();
		for( int i = 0; i < 6; ++i ) {
			DecisionSnapshot d = a.GetDecision()!;
			steps.Add( new BatchStep( d.Options[0], d.Prompt ) );
			a.Select( d.Version, d.Options[0] );
			expectedAlerts.AddRange( a.GetDecision()!.Alerts.Select( x => x.Text ) );
		}

		game.SelectBatch( game.GetDecision()!.Version, steps ).Applied.ShouldBe( 6 );
		Alert[] alerts = game.GetDecision()!.Alerts;

		alerts.Count( x => x.Kind == "batch" ).ShouldBe( 6 );
		alerts.Where( x => x.Kind != "batch" ).Select( x => x.Text ).ShouldBe( expectedAlerts );
		game.GetDecision()!.Alerts.ShouldBe( alerts ); // repeated reads show the same alerts
	}

	[Fact]
	public void Batch_StopsAtAPromptThatDoesNotMatch_WithoutAnsweringIt() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;

		BatchResult r = game.SelectBatch( d.Version, [new BatchStep( d.Options[0], "Some other prompt" )] );

		r.Applied.ShouldBe( 0 );
		r.StoppedBecause!.ShouldContain( "Some other prompt" );
		game.History.ShouldBeEmpty();
		game.GetDecision()!.Version.ShouldBe( d.Version );
		game.GetDecision()!.Alerts.ShouldBe( d.Alerts ); // untouched
	}

	[Fact]
	public void Batch_StopsAtAnOptionThatIsNotOffered_AfterApplyingTheStepsBefore() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;

		BatchResult r = game.SelectBatch( d.Version, [new BatchStep( d.Options[0] ), new BatchStep( "not an option" )] );

		r.Applied.ShouldBe( 1 );
		r.Requested.ShouldBe( 2 );
		r.StoppedBecause!.ShouldContain( "not an option" );
		game.GetDecision()!.Version.ShouldBeGreaterThan( d.Version );
	}

	[Fact]
	public void Batch_RejectsAStaleVersion() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;
		Should.Throw<ArgumentException>( () => game.SelectBatch( d.Version + 1, [new BatchStep( d.Options[0] )] ) );
		game.History.ShouldBeEmpty();
	}

	[Fact]
	public void Done_ThatDropsPlayedPowers_IsReportedAsSkipped_ButNotWhenTheyComeBackInTheSlowPhase() {
		var game = new AgentGame( new NewGameRequest( "Lightning's Swift Strike", "A", 1 ) );
		BatchResult r = game.SelectBatch( game.GetDecision()!.Version, [
			new( "Gain 3 Energy (group: Gain 3 Energy / Place Presence(1))" ),
			new( "1 energy => A8", "Select Presence" ),
			new( "Shatter Homesteads $2 (Slow)", "Play power card" ),
			new( "Harbingers of the Lightning $0 (Slow)", "Play power card" ),
			new( "Done", "Select Fast to resolve" ), // both are Slow: offered early by Swiftness, they come back in the Slow phase
		] );
		r.StoppedBecause.ShouldBeNull();
		DecisionSnapshot d = game.GetDecision()!;
		d.Alerts.ShouldNotContain( a => a.Kind == "skipped" );
		d.Prompt.ShouldStartWith( "Select Slow to resolve" );

		game.Select( d.Version, "Done" ); // now they are gone for the turn

		Alert skipped = game.GetDecision()!.Alerts.Single( a => a.Kind == "skipped" );
		skipped.Text.ShouldContain( "Shatter Homesteads" );
		skipped.Text.ShouldContain( "Harbingers of the Lightning" );
	}

	/// <summary> Lightning, board A, seed 1, round 1 played; returns at the round 2 "Play power card" prompt (presence placed). </summary>
	static AgentGame LightningAtRound2CardPlay() {
		var game = new AgentGame( new NewGameRequest( "Lightning's Swift Strike", "A", 1 ) );
		game.SelectBatch( game.GetDecision()!.Version, [
			new( "Gain 3 Energy (group: Gain 3 Energy / Place Presence(1))" ),
			new( "1 energy => A8", "Select Presence" ),
			new( "Shatter Homesteads $2 (Slow)", "Play power card" ),
			new( "Harbingers of the Lightning $0 (Slow)", "Play power card" ),
			new( "Shatter Homesteads $2 (Slow)", "Select Fast" ),
			new( "A8", "Shatter Homesteads" ),
			new( "Harbingers of the Lightning $0 (Slow)", "Select Fast" ),
			new( "A7", "Harbingers" ),
			new( "Done", "Push" ),
			new( "Gain 3 Energy (group: Gain 3 Energy / Place Presence(1))", "Select Growth" ),
		] ).StoppedBecause.ShouldBeNull();
		DecisionSnapshot d = game.GetDecision()!;
		d.Prompt.ShouldStartWith( "Select Presence" );
		game.Select( d.Version, d.Options[0] );
		game.GetDecision()!.Prompt.ShouldStartWith( "Play power card" );
		return game;
	}

	static Dictionary<string, Dictionary<string, int>> LandTokens( AgentGame game )
		=> PlayerObservation.Build( game )["spaces"]!.AsObject().ToDictionary(
			kv => kv.Key, kv => kv.Value!.AsObject().ToDictionary( t => t.Key, t => t.Value!.GetValue<int>() ) );

	[Fact]
	public void PredictInvaderPhase_MatchesWhatReallyHappens_AndLeavesTheGameAlone() {
		AgentGame game = LightningAtRound2CardPlay();
		var gs = game.GameState;
		ActionScope.Initialize( gs.RootScope );
		string stateBefore = gs.ToJson( new SpiritIsland.Serialization.GameStateSerializationContext( gs ) ).ToJsonString();
		var tokensBefore = LandTokens( game );
		int blightBefore = gs.BlightCard.BlightCount;
		DecisionSnapshot d = game.GetDecision()!;

		JsonObject p = InvaderPhasePrediction.Predict( game );

		// The live game is untouched.
		ActionScope.Initialize( gs.RootScope );
		gs.ToJson( new SpiritIsland.Serialization.GameStateSerializationContext( gs ) ).ToJsonString().ShouldBe( stateBefore );
		game.GetDecision()!.Version.ShouldBe( d.Version );

		// Now play it for real, doing nothing else: no cards, so the Invader Phase follows directly.
		game.Select( d.Version, "Done" );
		game.GetDecision()!.Prompt.ShouldBe( "Select Growth" ); // round 3

		var tokensAfter = LandTokens( game );
		var lands = p["lands"]!.AsObject();
		foreach( string land in tokensBefore.Keys.Union( tokensAfter.Keys ) ) {
			var before = tokensBefore.GetValueOrDefault( land ) ?? [];
			var after = tokensAfter.GetValueOrDefault( land ) ?? [];
			string[] realChanges = [.. before.Keys.Union( after.Keys ).Order()
				.Where( k => before.GetValueOrDefault( k ) != after.GetValueOrDefault( k ) )
				.Select( k => $"{k} {before.GetValueOrDefault( k )}->{after.GetValueOrDefault( k )}" )];
			(lands[land]?.GetValue<string>() ?? "").ShouldBe( string.Join( ", ", realChanges ), land );
		}
		ActionScope.Initialize( gs.RootScope );
		p["blightOnCard"]!.GetValue<string>().ShouldBe( $"{blightBefore} -> {gs.BlightCard.BlightCount}" );
		p["outcome"].ShouldBeNull();
	}

	[Fact]
	public void PredictInvaderPhase_DoesNotRevealTheBlightCard_WhenItWouldFlip() {
		AgentGame game = LightningAtRound2CardPlay();
		var gs = game.GameState;
		ActionScope.Initialize( gs.RootScope );
		gs.BlightCard.InitBlight( 1 ); // this round's ravages blight 2 lands, so blight on the card runs out
		BlightCard card = gs.BlightCard;
		card.CardFlipped.ShouldBeFalse();

		JsonObject p = InvaderPhasePrediction.Predict( game );
		string json = p.ToJsonString();

		p["outcome"]!.GetValue<string>().ShouldContain( "would flip" );
		json.ShouldNotContain( card.Name );
		json.ShouldNotContain( card.Description );
		foreach( IFearCard hidden in gs.Fear.Deck.Concat( gs.Fear.ActivatedCards ) )
			json.ShouldNotContain( ((IOption)hidden).Text );
		ActionScope.Initialize( gs.RootScope );
		gs.BlightCard.ShouldBeSameAs( card ); // the stand-in only ever lived in the copy
		card.CardFlipped.ShouldBeFalse();
	}

	[Fact]
	public void PredictInvaderPhase_DuringTheSlowPhase_PredictsNextRound() {
		var game = new AgentGame( Req( 3 ) );
		var rnd = new Random( 3 );
		DecisionSnapshot? d = game.GetDecision();
		for( int i = 0; i < 300 && d is not null && game.GameState.Phase != Phase.Slow; ++i ) {
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
			d = game.GetDecision();
		}
		game.GameState.Phase.ShouldBe( Phase.Slow );
		int round = game.GameState.RoundNumber;

		JsonObject p = InvaderPhasePrediction.Predict( game );

		(p["outcome"]?.GetValue<string>() ?? "").ShouldNotStartWith( "Simulation" ); // didn't fail or hang
		p["notIncluded"]![0]!.GetValue<string>().ShouldContain( "next round" );
		game.GameState.RoundNumber.ShouldBe( round );
		game.GameState.Phase.ShouldBe( Phase.Slow );
	}

	[Fact]
	public void PredictInvaderPhase_ReportsPendingFearCardsWithoutResolvingThem() {
		AgentGame game = LightningAtRound2CardPlay();
		var gs = game.GameState;
		ActionScope.Initialize( gs.RootScope );
		IFearCard earned = gs.Fear.Deck.Pop();
		gs.Fear.ActivatedCards.Push( earned );

		JsonObject p = InvaderPhasePrediction.Predict( game );

		p["notIncluded"]!.AsArray().Select( n => n!.GetValue<string>() ).ShouldContain( s => s.Contains( "1 earned fear card" ) );
		p.ToJsonString().ShouldNotContain( ((IOption)earned).Text );
		gs.Fear.ActivatedCards.Count.ShouldBe( 1 ); // still face down in the real game
	}

	[Fact]
	public void ObservationDiffs_RebuildEachFullObservation_ThroughoutAGame() {
		var game = new AgentGame( Req( 9 ) );
		var rnd = new Random( 9 );
		JsonObject first = game.Observations.Next( PlayerObservation.Build( game ), null );
		first["full"]!.GetValue<bool>().ShouldBeTrue();
		int id = first["observationId"]!.GetValue<int>();
		JsonObject known = WithoutIds( first );
		int smallerCount = 0, compared = 0;

		for( int i = 0; i < 120; ++i ) {
			DecisionSnapshot? d = game.GetDecision();
			if( d is null ) break;
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
			if( game.GetDecision() is null ) break;

			JsonObject actual = PlayerObservation.Build( game );
			JsonObject response = game.Observations.Next( (JsonObject)actual.DeepClone(), id );
			response["since"]!.GetValue<int>().ShouldBe( id );
			JsonObject changes = response["changes"]!.AsObject();

			known = ObservationHistory.Apply( known, changes );
			JsonNode.DeepEquals( known, actual ).ShouldBeTrue( $"after decision {i}: {changes.ToJsonString()}" );
			++compared;
			if( changes.ToJsonString().Length < actual.ToJsonString().Length ) ++smallerCount;
			id = response["observationId"]!.GetValue<int>();
		}
		compared.ShouldBeGreaterThan( 10 );
		smallerCount.ShouldBe( compared ); // a diff is never bigger than the full observation
	}

	static JsonObject WithoutIds( JsonObject response ) {
		var o = (JsonObject)response.DeepClone();
		o.Remove( "observationId" );
		o.Remove( "full" );
		return o;
	}

	[Fact]
	public void ObservationDiff_SendsAChangedLandWhole_AndAnEmptiedLandAsNull() {
		var before = JsonNode.Parse( """{"round":1,"spaces":{"A1":{"E@1":1,"T@2":1},"A2":{"D@2":2},"A3":{"E@1":1}}}""" )!.AsObject();
		var after = JsonNode.Parse( """{"round":1,"spaces":{"A1":{"E@1":2,"T@2":1},"A2":{"D@2":2},"A4":{"E@1":1}}}""" )!.AsObject();

		JsonObject diff = ObservationHistory.Diff( before, after );

		diff.ToJsonString().ShouldBe( """{"spaces":{"A1":{"E@1":2,"T@2":1},"A4":{"E@1":1},"A3":null}}""" );
		JsonNode.DeepEquals( ObservationHistory.Apply( before, diff ), after ).ShouldBeTrue();
	}

	[Fact]
	public void Observation_ComesBackFull_ForAnIdThatIsNoLongerRemembered() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		int oldest = game.Observations.Next( PlayerObservation.Build( game ), null )["observationId"]!.GetValue<int>();
		for( int i = 0; i < 3; ++i ) game.Observations.Next( PlayerObservation.Build( game ), null );

		JsonObject r = game.Observations.Next( PlayerObservation.Build( game ), oldest );
		r["full"]!.GetValue<bool>().ShouldBeTrue();
		r["spaces"].ShouldNotBeNull();

		JsonObject unknown = game.Observations.Next( PlayerObservation.Build( game ), 999 );
		unknown["full"]!.GetValue<bool>().ShouldBeTrue();

		// The one just sent is remembered: nothing changed since it.
		int latest = unknown["observationId"]!.GetValue<int>();
		game.Observations.Next( PlayerObservation.Build( game ), latest )["changes"]!.AsObject().Count.ShouldBe( 0 );
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
	public void EarnedButUnresolvedFearCard_OnlyItsCountIsVisible() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		var gs = game.GameState;

		// Earn a card the way the engine does: it sits face down until the Invader Phase flips it.
		IFearCard earned = gs.Fear.Deck.Pop();
		gs.Fear.ActivatedCards.Push( earned );

		var fear = PlayerObservation.Build( game )["fear"]!.AsObject();
		fear["earnedFearCardsPending"]!.GetValue<int>().ShouldBe( 1 );
		fear.ContainsKey( "revealedCards" ).ShouldBeFalse();
		fear.ToJsonString().ShouldNotContain( ((IOption)earned).Text );
	}

	[Fact]
	public void UnflippedBlightCard_NameAndEffectAreHidden() {
		var game = new AgentGame( Req() );
		DecisionSnapshot d = game.GetDecision()!;
		BlightCard card = game.GameState.BlightCard;
		card.CardFlipped.ShouldBeFalse();

		var blight = PlayerObservation.Build( game )["blight"]!.AsObject();
		blight["card"].ShouldBeNull();
		blight["blightedIslandEffect"].ShouldBeNull();
		blight["blightRemainingOnCard"]!.GetValue<int>().ShouldBeGreaterThan( 0 ); // the printed count stays visible

		// ...and not leaked through the observation as a whole or through the alerts either
		PlayerObservation.Build( game ).ToJsonString().ShouldNotContain( card.Name );
		d.Alerts.ShouldAllBe( a => !a.Text.Contains( card.Name ) );
	}

	[Fact]
	public void Observation_ReportsSpiritAndIsland() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		var obs = PlayerObservation.Build( game );

		obs["spirit"]!["hand"]!.AsArray().Count.ShouldBeGreaterThan( 0 );
		obs["spirit"]!["hand"]![0]!.AsObject().ContainsKey( "instructions" ).ShouldBeFalse(); // full text is in the static info / get_card_details
		var spaces = obs["spaces"]!.AsObject();
		spaces.Count.ShouldBeGreaterThan( 0 );
		spaces.ShouldAllBe( kv => kv.Value!.AsObject().Count > 0 ); // empty lands are left out
		obs["spirit"]!["presence"]!["energyTrackRevealed"]!.GetValue<int>().ShouldBe( 1 );
	}

	[Fact]
	public void StaticInfo_HoldsLayoutSpiritRulesAndStartingCards() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		var info = PlayerObservation.BuildStatic( game );

		info["setup"]!["spirit"]!.GetValue<string>().ShouldBe( RiverSurges.Name );
		info["setup"]!["adversary"].ShouldBeNull(); // not the "No Adversary" placeholder
		info["lands"]!.AsArray().Count.ShouldBe( 9 ); // ocean + 8 lands
		info["lands"]![1]!["adjacent"]!.AsArray().Count.ShouldBeGreaterThan( 0 );
		info["spirit"]!["innatePowers"]![0]!["tiers"]!.AsArray().Count.ShouldBeGreaterThan( 0 );
		info["spirit"]!["presenceTracks"]!["energy"]!.AsArray().Count.ShouldBeGreaterThan( 1 );
		info["startingCards"]!.AsArray().Select( c => c!["name"]!.GetValue<string>() )
			.ShouldBe( game.GameState.Spirits.Single().Hand.Select( c => c.Title ), ignoreOrder: true );
		info["startingCards"]![0]!["instructions"].ShouldNotBeNull();
	}

	[Fact]
	public void StaticVersion_MatchesObservation_AndStaysTheSameWhilePlaying() {
		var game = new AgentGame( Req() );
		game.GetDecision();
		string version = PlayerObservation.BuildStatic( game )["staticVersion"]!.GetValue<string>();
		PlayerObservation.Build( game )["staticVersion"]!.GetValue<string>().ShouldBe( version );

		// Playing (growth, presence placed, cards drawn and played) doesn't change what's in the static info.
		var rnd = new Random( 6 );
		for( int i = 0; i < 40; ++i ) {
			DecisionSnapshot? d = game.GetDecision();
			if( d is null ) break;
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
		}
		if( !game.IsOver )
			PlayerObservation.Build( game )["staticVersion"]!.GetValue<string>().ShouldBe( version );
	}

	[Fact]
	public void StaticInfo_DoesNotRevealHiddenCards() {
		var game = new AgentGame( Req( 2, "England", 1 ) );
		game.GetDecision();
		var gs = game.GameState;
		string json = PlayerObservation.BuildStatic( game ).ToJsonString();

		foreach( IFearCard hidden in gs.Fear.Deck )
			json.ShouldNotContain( ((IOption)hidden).Text );
		json.ShouldNotContain( gs.BlightCard.Name );

		var adversary = PlayerObservation.BuildStatic( game )["setup"]!["adversary"]!;
		adversary["name"]!.GetValue<string>().ShouldBe( "England" );
		adversary["rules"]!.GetValue<string>().ShouldNotBeNullOrWhiteSpace();
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

		var growth = PlayerObservation.BuildStatic( game )["spirit"]!["growthGroups"]!.AsArray();
		growth.Count.ShouldBeGreaterThan( 0 );
	}
}
