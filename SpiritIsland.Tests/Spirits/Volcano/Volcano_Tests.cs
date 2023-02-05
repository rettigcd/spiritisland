namespace SpiritIsland.Tests.Spirits.Volcano;

public class Volcano_Tests {

	static (VolcanoLoomingHigh, GameState, Board) Init() {
		VolcanoLoomingHigh spirit = new VolcanoLoomingHigh();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		return (spirit, gameState, board);
	}

	[Trait( "Spirit", "SetupAction" )]
	[Fact]
	public void HasSetUp() {
		var fxt = new ConfigurableTestFixture { Spirit = new VolcanoLoomingHigh() };
		fxt.GameState.Initialize();
		fxt.Spirit.GetAvailableActions( Phase.Init ).Count().ShouldBe( 1 );
	}

	[Theory]
	[Trait("SpecialRule", VolcanicPeaksTowerOverTheLandscape.Name )]
	[InlineData(2,"A6")]
	[InlineData(3,"A1,A5,A6,A8")]
	public async Task Range(int presenceCount, string expectedOptions ) {
		// Your Power Cards gain +1 Range if you have 3 or more Presence in the origin land.

		// Given: Volcano
		var (spirit, gameState, board) = Init();
		Space a6 = board[6];

		// Given: Only Presence is on A5
		foreach(SpaceState ss in spirit.Presence.ActiveSpaceStates( gameState ).ToArray())
			SpiritExtensions.Adjust( spirit.Presence, ss, -1 );
		SpiritExtensions.Adjust( spirit.Presence, gameState.Tokens[a6], presenceCount );

		// When: Activating a Range-0 card
		await using UnitOfWork actionScope = gameState.StartAction(ActionCategory.Spirit_Power);
		_ = PowerCard.For<MesmerizedTranquility>().ActivateAsync( spirit.BindMyPowers(gameState, actionScope) );

		spirit.NextDecision().HasOptions( expectedOptions );
	}

	[Trait( "SpecialRule", VolcanicPeaksTowerOverTheLandscape.Name )]
	[Fact]
	public void Growth_DoesntGetRangeExtension() {
		// Your Power Cards gain +1 Range if you have 3 or more Presence in the origin land.

		// Given: Volcano
		var (spirit,gameState,board) = Init();
		Space a6 = board[6];

		// Given: 3 presence on A6
		foreach(SpaceState ss in spirit.Presence.ActiveSpaceStates( gameState ).ToArray())
			SpiritExtensions.Adjust( spirit.Presence, ss, -1 );
		SpiritExtensions.Adjust( spirit.Presence, gameState.Tokens[a6], 3 );

		// When: Activating Growth Range-0 presence
		_ = spirit.DoGrowth(gameState);
		spirit.NextDecision().HasPrompt("select growth").ChooseFirst( "PlacePresence(0,Mountain)" );
		spirit.NextDecision().HasPrompt( "Select Presence to place" ).Choose( "2 energy" );

		// Then only option available is A6 (range-0)
		spirit.NextDecision().HasPrompt( "Where would you like to place your presence?" )
			.HasOptions( a6.Text );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public async Task PowerDestroyingPresence_CausesDamage() {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[5]];

		// Given: explorers, presence, and dahan
		space.InitDefault( Human.Explorer, 3 );
		space.Dahan.Init( 3 );
		SpiritExtensions.Adjust( spirit.Presence, space, 3 );

		//  When: they destroying presence via Powercard
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<GrowthThroughSacrifice>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasPrompt( "Select presence to destroy" ).HasOptions( space.Space.Text ).Choose( space.Space );

		//  Then: cause 1 damage 
		spirit.NextDecision().HasPrompt( "Damage (1 remaining)" ).HasOptions( "E@1" ).Choose( "E@1" );
		space.Summary.ShouldBe( $"1D@1,2D@2,2E@1" );

		//   And: wrap up
		spirit.NextDecision().HasPrompt( "Select location to Remove Blight OR Add Presence" ).HasOptions( "A5" ).Choose( "A5" );

		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public void RavageBlight_Causes1Damage() {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[5]];

		// Given: 3 explorers, 3 presence
		space.InitDefault( Human.Explorer, 3 );
		SpiritExtensions.Adjust( spirit.Presence, space, 3 );

		//   And: Island Won't blight
		gameState.IslandWontBlight();

		//  When: they ravage and cause blight, destroying 1 presence
		Task task = space.DoARavage();

		//  Then: destroyed presence causes 1 damage to explorer
		ApplyDamageToExplorers(spirit,task,1, space );
		space.Summary.ShouldBe("1B,2E@1");

		Assert_IsDone(spirit,task);
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Fact]
	public async Task PeeksTower_ExtendsRangeFor_BargainsOfPowerAndProtection() {
		var (spirit, gameState, board) = Init();
		SpaceState targetSpace = gameState.Tokens[board[5]];
		SpaceState dahanSpace = gameState.Tokens[board[2]];

		// Given: Enough Presence to trigger Tower rule
		SpiritExtensions.Adjust( spirit.Presence, targetSpace, 5 );
		//   And: dahan in target & dahan space
		targetSpace.Dahan.Init( 2 );
		dahanSpace.Dahan.Init( 2 );

		//  When: they destroying presence via Powercard
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<BargainsOfPowerAndProtection>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasPrompt( "Bargains of Power and Protection: Target Space" ).Choose( "A5" );
		spirit.NextDecision().HasPrompt( "Select presence to remove from game." ).HasOptions( "A5" ).Choose( "A5" );

		//  Then: range-2 item has a defend.
		dahanSpace[Token.Defend].ShouldBe(2); // 1 per dahan
		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData(7, "A5,A6,A7,A8" )]        // Target space is not on Tower => normal range
	[InlineData( 8, "A1,A4,A5,A6,A7,A8" )] // Target space is on Tower => range boost
	public async Task PeeksTower_ExtendsRangeFor_UtterACurseOfDreadAndBone( int towerSpaceNum, string expectedRangeOptions) {
		var (spirit, gameState, board) = Init();
		SpaceState towerSpace = gameState.Tokens[board[towerSpaceNum]];
		SpaceState targetSpace = gameState.Tokens[board[8]];

		// Given: Enough Presence to trigger Tower rule
		SpiritExtensions.Adjust( spirit.Presence, towerSpace, 5 );
		//   And: enough elements to trigger threshold
		spirit.Configure().Elements("3 moon,2 animal");
		//   And: 1 blight in target land
		targetSpace.Init( Token.Blight, 1 );

		//  When: Utter a curse
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<UtterACurseOfDreadAndBone>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().Choose( targetSpace.Space );
		spirit.NextDecision().HasPrompt( "Select Power Option" ).HasOptions( "Add Badland,Add Disease,Add Strife" ).Choose( "Add Disease" );
		//  Then: if the tower==target, then large range, else smaller range
		spirit.NextDecision().HasPrompt( "Add Disease" ).HasOptions( expectedRangeOptions ).ChooseFirst();

		spirit.NextDecision().HasPrompt( "Select land for 1 Damage" ).HasOptions( "A5,A6,A7" ).ChooseFirst(); // Adjacent, NOT Range

		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData( 2, "A8" )]        // range-0
	[InlineData( 3, "A5,A6,A7,A8" )] // range-1
	public async Task PeeksTower_ExtendsRangeFor_UnleashATorrent( int presenceInTower, string expectedRangeOptions ) {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[8]];

		// Given: presence tower
		SpiritExtensions.Adjust( spirit.Presence, space, presenceInTower );
		//   And: 1 energy
		spirit.Energy=1;

		//  When: Unleash a Torrent
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<UnleashATorrentOfTheSelfsOwnEssence>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasPrompt( "Select Power Option" )
			.HasOptions( "Gain 4 energy, Forget a Power Card to gain 4 more,Pay X Energy (min 1) to deal X Damage in a land at range 0" )
			.Choose( "Pay X Energy (min 1) to deal X Damage in a land at range 0" );
		spirit.NextDecision().HasPrompt( "Pay 1 energy/damage." ).HasOptions( "1,0" ).Choose( "1" );

		//  Then: able to target range-0 or range-1
		spirit.NextDecision().HasPrompt( "1 Damage" ).HasOptions( expectedRangeOptions ).ChooseFirst();

		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData( 2, "A5,A6,A7,A8" )]       // range-1
	[InlineData( 3, "A1,A4,A5,A6,A7,A8" )] // range-2
	public async Task PeeksTower_ExtendsRangesFor_PerilsOfTheDeepIsland( int presenceInTower, string expectedRangeOptions ) {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[8]];

		// Given: presence tower
		SpiritExtensions.Adjust( spirit.Presence, space, presenceInTower );
		//   And: 1 energy
		spirit.Energy = 1;

		//  When: Perils of the Deepest Island
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<PerilsOfTheDeepestIsland>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().Choose(space.Space);

		//  Then: range is adjusted for adding beasts
		spirit.NextDecision().HasPrompt( "Add beast" ).HasOptions( expectedRangeOptions ).ChooseFirst();

		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public async Task ExplosiveErruption_Level0_CausesDamage() {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[5]];

		// Given: 10 explorers 10 dahan, 4 presence, n-badlands
		space.InitDefault( Human.Explorer, 10 );
		space.Dahan.Init( 10 );
		SpiritExtensions.Adjust( spirit.Presence, space, 4 );
		//   But: not enough elements to trigger Tier-1
		spirit.Configure().Elements( "0 fire,0 earth" );

		//  When: they trigger Explosive Erruption in target
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = InnatePower.For<ExplosiveEruption>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasPrompt( "Explosive Eruption: Target Space" ).Choose( space.Space );
		//   And: Destroy 2 presence
		spirit.NextDecision().HasPrompt( "# of presence to destroy?" ).HasOptions( "4,3,2,1,0" ).Choose( "2" );

		//  Then: damage invaders in target
		ApplyDamageToExplorers( spirit, task, 2, space );
		int remaining = 8;
		//   And: damages dahan INEFFICIENTLY
		space.Summary.ShouldBe( $"2D@1,{remaining}D@2,{remaining}E@1" );

		//   And: doesn't trigger adjacent land or anything else
		Assert_IsDone( spirit, task );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // does not Extend Explosive Erruption
	[Trait( "Token", "Badlands" )]
	[Theory]
	[InlineData( 0 )]
	[InlineData( 2 )]
	public async Task ExplosiveErruption_Level1_CausesDamage( int badlandsCount ) {
		var (spirit, gameState, board) = Init();
		SpaceState space = gameState.Tokens[board[5]];
		SpaceState adjacent = gameState.Tokens[board[7]];

		// Given: 10 explorers 10 dahan, 10 presence, n-badlands
		space.InitDefault( Human.Explorer, 10 );
		space.Dahan.Init( 10 );
		SpiritExtensions.Adjust( spirit.Presence, space, 10 );
		space.Init( Token.Badlands, badlandsCount );
		//   And: 10 exploreres, 10 dahan in adjacent
		adjacent.InitDefault( Human.Explorer, 10 );
		adjacent.Dahan.Init( 10 );
		adjacent.Init( Token.Badlands, badlandsCount );
		//   And: 2 fire and 2 earth
		spirit.Configure().Elements( "2 fire,2 earth" );

		//  When: they trigger Explosive Erruption in target
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = InnatePower.For<ExplosiveEruption>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasPrompt( "Explosive Eruption: Target Space" ).Choose( space.Space );
		//   And: Destroy 2 presence
		spirit.NextDecision().HasPrompt( "# of presence to destroy?" ).HasOptions( "10,9,8,7,6,5,4,3,2,1,0" ).Choose( "2" );

		//   And: damage invaders in target
		ApplyDamageToExplorers( spirit, task, 2 + badlandsCount, space );
		int remaining = 10 - (2 + badlandsCount);
		//  Then: VolcanicPeaksTowerOverTheLandscape does not extend range to A2 & A3
		spirit.NextDecision().HasPrompt( "Apply 2 damage to" ).HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( adjacent.Space );
		//   And: damages invaders in adjacent
		ApplyDamageToExplorers( spirit, task, 2 + badlandsCount, adjacent );

		//  Then: dahan were damaged INEFFICIENTLY in target space
		space.Summary.ShouldBe( $"{2 + badlandsCount}D@1,{remaining}D@2,{remaining}E@1" + (badlandsCount == 0 ? "" : $",{badlandsCount}M") );
		//   And: dahan were left alone in adjacent
		adjacent.Summary.ShouldBe( $"10D@2,{remaining}E@1" + (badlandsCount == 0 ? "" : $",{badlandsCount}M") );

		Assert_IsDone( spirit, task );
	}


	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 4 )]
	[InlineData( 6 )]
	[InlineData( 10 )]
	public async Task ExplosiveEruption( int presenceDestroyed ) {
		var (spirit, gameState, board) = Init();
		SpaceState targetSpace = gameState.Tokens[board[8]];
		SpaceState adjPresence = gameState.Tokens[board[7]];
		SpaceState adjBlight = gameState.Tokens[board[6]];

		// Given: enough elements to trigger all
		spirit.Configure().Elements("5 fire,3 air,5 earth");

		//   And: TargetSpace => 12 presence, 20 explorers
		SpiritExtensions.Adjust( spirit.Presence, targetSpace, 12 );
		targetSpace.InitDefault( Human.Explorer, 20 );

		//   And: AdjPresence => 1 presence, 20 explorers
		SpiritExtensions.Adjust( spirit.Presence, adjPresence, 1 );
		adjPresence.InitDefault( Human.Explorer, 20 );

		//   And: AdjBlight => 1 blight, 10 explorers
		adjBlight.Blight.Init(1);
		adjBlight.InitDefault( Human.Explorer, 20 );

		//  And: Island won't blight
		gameState.IslandWontBlight();

		// When: activate Innate
		await using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = InnatePower.For<ExplosiveEruption>().ActivateAsync( spirit.BindMyPowers( gameState, actionScope ) );
		spirit.NextDecision().HasOptions("A7,A8").Choose( targetSpace.Space );

		//  And: destroy presence
		spirit.NextDecision().HasPrompt( "# of presence to destroy?" ).HasOptions( "12,11,10,9,8,7,6,5,4,3,2,1,0" ).Choose( presenceDestroyed.ToString() );

		// Then: trigger Level 0
		if( 1 <= presenceDestroyed) { 
			ApplyDamageToExplorers( spirit, task, presenceDestroyed, targetSpace );
			targetSpace.Summary.ShouldBe($"{20- presenceDestroyed}E@1" );
		}

		//  And: Level 1
		if(2 <= presenceDestroyed) {
			spirit.NextDecision().HasPrompt( $"Apply {presenceDestroyed} damage to" ).HasOptions( "A5,A6,A7,A8" ).Choose(adjBlight.Space);
			ApplyDamageToExplorers(spirit, task, presenceDestroyed, adjBlight );
		}
		//  And: Level 2
		int earnedFear = gameState.Fear.EarnedFear + gameState.Fear.ActivatedCards.Count * 4;
		earnedFear.ShouldBe( 4<=presenceDestroyed ? presenceDestroyed : 0);

		//  And: Level 3
		if(6 <= presenceDestroyed) {
			ApplyDamageToExplorers( spirit, task, 4, targetSpace ); // A8
			ApplyDamageToExplorers( spirit, task, 4, adjBlight );   // A6
			ApplyDamageToExplorers( spirit, task, 4, adjPresence ); // A7

			// target
			spirit.Presence.CountOn( targetSpace ).ShouldBe( 12 - presenceDestroyed ); // if this is 1 too low, blight destroyed presence
			targetSpace.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed+4)}E@1" );
			// adj-blight
			adjBlight.Summary.ShouldBe( $"1B,{20-(presenceDestroyed+4)}E@1" );
			// adj-presence
			adjPresence.Summary.ShouldBe( $"{20-4}E@1" );
		}

		if(10 <= presenceDestroyed) {
			// When: applying 4 more damage
			ApplyDamageToExplorers( spirit, task, 4, targetSpace ); // A8
			ApplyDamageToExplorers( spirit, task, 4, adjBlight );   // A6
			ApplyDamageToExplorers( spirit, task, 4, adjPresence ); // A7

			// When: Adding blight destroys presence and causes 1 more damage
			ApplyDamageToExplorers( spirit, task, 1, adjPresence ); // A7

			// Then: target
			spirit.Presence.CountOn( targetSpace ).ShouldBe( 12 - presenceDestroyed ); // if this is 1 too low, blight destroyed presence
			targetSpace.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4 + 4)}E@1" );

			// Then: adj-blight
			adjBlight.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4 + 4)}E@1" );

			// Then: adj-presence
			spirit.Presence.CountOn( adjPresence ).ShouldBe( 0 );
			adjPresence.Summary.ShouldBe( $"1B,{20 - (4 + 4 + 1)}E@1" );

		}

		Assert_IsDone( spirit, task );
	}

	// 1Powered By the Furnace of the Earth

	#region private helpers

	static void ApplyDamageToExplorers( Spirit spirit, Task task, int expectedDamage, SpaceState onSpace ) {
		while(0 < expectedDamage) {
			string prompt = $"Damage ({expectedDamage} remaining)";
			task.IsCompleted.ShouldBeFalse( nameof(ApplyDamageToExplorers) + "() => " + prompt );
			spirit.NextDecision()
				.HasPrompt( prompt )
				.IsForSpace( onSpace.Space )
				.HasOptions( "E@1" )
				.Choose( "E@1" );
			--expectedDamage;
		}
	}

	static void Assert_IsDone( VolcanoLoomingHigh spirit, Task task ) {
		if(task.IsCompleted) return;
		// ONLY call this if not completed. Otherwise it will block...
		task.IsCompleted.ShouldBeTrue( spirit.NextDecision().Format() ); 
	}

	#endregion
}
