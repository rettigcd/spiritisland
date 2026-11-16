using SpiritIsland.FeatherAndFlame;

namespace SpiritIsland.Tests.Spirits.Finder;


public class Finder_Tests : BoardAGame {
	public Finder_Tests():base(new FinderOfPathsUnseen()){
		_gameState.Initialize();
	}

	[Fact]
	public async Task PlacePresence(){

		// When: Finder places presence
		await new PlacePresence(1).ActAsync(_spirit).AwaitUser( u=>{
			u.NextDecision.HasPrompt("Select Presence to place")
				//.HasOptions("sun energy,earth energy,FoPU on A3,FoPU on A1") => "A1,A2,A3,A4,A5,A6"
				.Choose("sun energy => A2");
		}).ShouldComplete();

		// Then: Sun Energy Slot should be revealed
		_spirit.Presence.CoverOptions.Select(s=>s.Code).Join(",").ShouldBe("sun energy,1 cardplay"); // 0 energy,1 cardplay

		//  And: should have Sun Element
		_spirit.Elements.Summary(true).ShouldBe("1 sun");
	}

	[Fact]
	public async Task Revealing2WaterSlot_Generates2Energy(){

		// When: Finder Grows: G2,G2 (taking from: Sun then 2-water)
		await Do_Growth2("sun energy", "A1");
		await Do_Growth2("2,water energy", "A1");

		// Then: should have Sun & Water
		_spirit.Elements.Summary(true).ShouldBe("1 sun 1 water");
		//  And: 2 energy
		_spirit.Energy.ShouldBe(2);
	}

	[Fact]
	public async Task DestroyPresence_OfLastPresence_LosesImmediately() {
		// Given: exactly 1 presence on the island
		foreach(var space in _spirit.Presence.Lands.ToArray())
			space.Init(_spirit.Presence.Token,0);
		_gameState.Island.Boards[0][3].ScopeSpace.Init(_spirit.Presence.Token,1);

		// When: asked to destroy 3
		// Then: the game is lost as soon as the last one is destroyed (no 2nd prompt, no crash)
		var ex = await Should.ThrowAsync<GameOverException>( async () =>
			await Cmd.DestroyPresence(3).ActAsync(_spirit).AwaitUser( u=>
				u.NextDecision.HasPrompt("Select Presence to Destroy").Choose("FoPU on A3")
			)
		);
		_spirit.Presence.IsOnIsland.ShouldBeFalse();
	}

	[Fact]
	public async Task OpenTheWays_WithPresenceOnOnlyOneLand_DoesNothing() {
		// Given: presence on only 1 land
		foreach(var space in _spirit.Presence.Lands.ToArray())
			space.Init(_spirit.Presence.Token,0);
		_gameState.Island.Boards[0][3].ScopeSpace.Init(_spirit.Presence.Token,1);

		// When: Open the Ways is used (2 lands are needed to make adjacent)
		// Then: it completes without prompting or throwing, and no gateway is made
		IActionFactory openTheWays = ((IOwnedActionFactories)_spirit).ResolveActionFactory("openTheWays");
		await openTheWays.ActivateAsync(_spirit).ShouldComplete();
		((FinderOfPathsUnseen)_spirit).GatewayToken.ShouldBeNull();
	}

	[Fact]
	public async Task ACircuitousAndWendingJourney_PushesHalfOfEachKind() {
		// (used to throw "Cannot set Quota twice": it set a quota per kind, and a selector takes only one)
		// Given: a land with presence, 2 explorers and 2 dahan
		Space a3 = _gameState.Tokens[_gameState.Island.Boards[0][3]];
		a3.Given_HasTokens("2E@1,2D@2");

		// When: the card is used on it
		await _spirit.When_ResolvingCard<ACircuitousAndWendingJourney>( u => {
			u.NextDecision.HasPrompt( "A Circuitous and Wending Journey: Target Space" ).Choose( "A3" );
			// Then: up to half of each kind may be pushed - 1 explorer, 1 dahan, 1 presence
			u.NextDecision.HasPrompt( "Push up to (3)" )
				.HasOptions( "D@2 on A3 => A2,D@2 on A3 => A4,E@1 on A3 => A2,E@1 on A3 => A4,FoPU on A3 => A2,FoPU on A3 => A4,Done" )
				.Choose( "E@1 on A3 => A2" );
			// And: each kind has its own allowance - the explorer is used up, the others are still offered
			u.NextDecision.HasPrompt( "Push up to (2)" )
				.HasOptions( "D@2 on A3 => A2,D@2 on A3 => A4,FoPU on A3 => A2,FoPU on A3 => A4,Done" )
				.Choose( "Done" );
		} ).ShouldComplete();
	}

	async Task Do_Growth2( string fromTrack, string destination ){
		await _spirit.DoGrowthForTestsAsync(_gameState).AwaitUser( u=>{
			u.NextDecision.HasPrompt("Select Growth").Choose("Place Presence(1)");
			u.NextDecision.HasPrompt("Select Presence to place")
				.Choose($"{fromTrack} => {destination}");
		}).ShouldComplete();
	}
}