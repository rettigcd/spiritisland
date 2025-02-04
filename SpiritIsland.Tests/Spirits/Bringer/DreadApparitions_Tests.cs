namespace SpiritIsland.Tests.Spirits.BringerNS;

[Collection("BaseGame Spirits")]
public class DreadApparitions_Tests {

	public DreadApparitions_Tests() {}

	[Fact]
	public async Task DirectFear_GeneratesDefend() {
		Bringer spirit = new Bringer();
		Board board = Boards.A;
		SpaceSpec targetSpace = board[5];
		var gs = new SoloGameState( spirit, board );

		// Given: DA run
		// await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );
		spirit.Given_IsOn(targetSpace); 
		targetSpace.Given_HasTokens("2T@2");
		await spirit.When_ResolvingCard<DreadApparitions>(u=>u.Choose(targetSpace.Label));

		// When: generating 2 fear
		await spirit.Target(targetSpace).AddFear(2);

		// Then: also get 3 defend
		targetSpace.ScopeSpace.Defend.Count.ShouldBe( 2+1 );// +1 from D.A.
	}

	[Fact]
	public async Task TownDamage_Generates2Defend() {

		Bringer spirit = new Bringer();
		Board board = Boards.A;
		SpaceSpec targetSpace = board[5];
		GameState gs = new SoloGameState( spirit, board );
		gs.AddIslandMod(new ToDreamAThousandDeaths(spirit)); // Initialized
		gs.DisableBlightEffect();

		// Given: 
		targetSpace.Given_HasTokens("1T@2");
		spirit.Given_IsOn(targetSpace);
		await spirit.When_ResolvingCard<DreadApparitions>(u => u.Choose(targetSpace.Label));

		// When: destroying the town with Power
		await using var actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, spirit);
		await spirit.Target(targetSpace).Invaders.DestroyNOfClass(1, Human.Town).AwaitUser(user => {
			user.NextDecision.HasPrompt("Push T@2 to")
				.HasOptions("A1,A4,A6,A7,A8")
				.Choose("A4");

		});

		// InSpiritPowerScope

		// Then: 2 fear should have triggered 2 defend
		targetSpace.ScopeSpace.Defend.Count.ShouldBe( 2+1 );

	}

	// Generate 5 DATD fear by 'killing' a city - should defend 5
	[Fact]
	public async Task CityDamage_Generates5Defend() {
		Spirit spirit = new Bringer();
		Board board = Boards.A;

		SpaceSpec targetSpace = board[5];
		GameState gs = new SoloGameState( spirit, board );
		gs.AddIslandMod(new ToDreamAThousandDeaths(spirit)); // Initialized
		gs.DisableBlightEffect();

		// Given: City on A5
		targetSpace.Given_HasTokens("1C@3");
		spirit.Given_IsOn(targetSpace);
		//  And: Spirit played Dread Apparitions on A5
		await spirit.When_ResolvingCard<DreadApparitions>(u => u.Choose(targetSpace.Label));

		// When: destroying city
		// await spirit.When_TargetingSpace(targetSpace, ctx => ctx.Invaders.DestroyNOfClass( 1, Human.City ) );
		await using( await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, spirit) ) {
			await spirit.Target(targetSpace).Invaders.DestroyNOfAnyClass(1, Human.City);
		}

		// Then: 5 fear should have triggered 2 defend
		board[5].ScopeSpace.Defend.Count.ShouldBe( 5+1 );// Dread Apparitions has 1 fear

	}

	[Fact]
	public async Task DahanDamage_Generates0() {

		Bringer spirit = new Bringer();
		Board board = Boards.A;
		SpaceSpec targetSpace = board[5];
		GameState gs = new SoloGameState( spirit, board );
		Space space = targetSpace.ScopeSpace;
		gs.DisableBlightEffect();
		gs.IslandWontBlight();


		// Given: has 1 city and lots of dahan
		space.Given_HasTokens("1C@3,10D@2");
		spirit.Given_IsOn(space);
		//   And: used Dread Apparitions
		await spirit.When_ResolvingCard<DreadApparitions>(u => u.Choose(targetSpace.Label));

		// When: dahan destroy the city
		await space.SpaceSpec.When_CardRavages();

		// Then: 2 fear from city
		SpiritIsland.Fear fear = gs.Fear;
		int actualFear = fear.EarnedFear + 4 * fear.ActivatedCards.Count;
		actualFear.ShouldBe(2+1);// normal (1 from Dread Apparitions)
		//  And: 1 defend bonus
		space.Defend.Count.ShouldBe( 1 ); // from dread apparitions
	}

	[Fact]
	public async Task FearInOtherLand_Generates0() {
		Bringer spirit = new Bringer();
		Board board = Boards.A; 
		var gs = new SoloGameState( spirit, board );
		var targetSpace = board[5];
		var otherSpace = board[1];

		// Given: has 1 city and lots of dahan
		targetSpace.ScopeSpace.Given_HasTokens("1C@3,10D@2");
		//   And: spirit is withing range of targt space
		spirit.Given_IsOn(targetSpace);
		//   And: triggered DA
		await spirit.When_ResolvingCard<DreadApparitions>(u => u.Choose(targetSpace.Label));

		// When: Power causes fear in a different land
		await otherSpace.ScopeSpace.AddFear(5);

		// Then: no defend bonus
		board[5].ScopeSpace.Defend.Count.ShouldBe( 1+0 );// 1=>Dread Apparitions
	}

}