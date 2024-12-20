namespace SpiritIsland.Tests.Spirits.Volcano;

public class Volcano_Tests {

	static (VolcanoLoomingHigh, GameState, Board) Init() {
		VolcanoLoomingHigh spirit = new VolcanoLoomingHigh();
		Board board = Boards.A;

		GameState gameState = new SoloGameState( spirit, board );
		return (spirit, gameState, board);
	}

	public Volcano_Tests() {
//		ActionScope.Initialize();
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
		SpaceSpec a6 = board[6];

		// Given: Only Presence is on A5
		foreach(Space ss in spirit.Presence.Lands.ToArray())
			spirit.Given_IsOn( ss, 0 );
		spirit.Given_IsOn( gameState.Tokens[a6], presenceCount );

		// When: Activating a Range-0 card
		await spirit.When_ResolvingCard<MesmerizedTranquility>( (user) => {

			// Then: has expected range
			user.NextDecision.HasOptions( expectedOptions ).Choose("A6");
		} );

	}

	[Trait( "SpecialRule", VolcanicPeaksTowerOverTheLandscape.Name )]
	[Fact]
	public void Growth_DoesntGetRangeExtension() {
		// Your Power Cards gain +1 Range if you have 3 or more Presence in the origin land.

		// Given: Volcano
		var (spirit,gameState,board) = Init();
		SpaceSpec a6 = board[6];

		// Given: 3 presence on A6
		foreach(Space ss in spirit.Presence.Lands.ToArray())
			spirit.Given_IsOn( ss, 0 );
		spirit.Given_IsOn( gameState.Tokens[a6], 3 );

		// When: Activating Growth Range-0 presence
		_ = spirit.DoGrowth(gameState);
		spirit.NextDecision().HasPrompt("select growth").ChooseFirst( "Place Presence(0,Mountain)" );
		// Then only option available is A6 (range-0)
		spirit.NextDecision().HasPrompt( "Select Presence to place" ).ChooseFrom( "2 energy" ).HasToOptions( a6.Label );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public async Task PowerDestroyingPresence_CausesDamage() {
		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[5]];

		// Given: explorers, presence, and dahan
		space.Given_HasTokens("3E@1,3D@2");
		spirit.Given_IsOn(space, 3);

		//  When: they destroying presence via Powercard
		await spirit.When_ResolvingCard<GrowthThroughSacrifice>( (user) => {
			user.NextDecision.HasPrompt( "Select presence to destroy" ).HasOptions( "VLH" ).Choose( "VLH" );
			// Expected prompt 'Select presence to destroy' not found in Select location to Remove Blight OR Add Presence:A5

			//  Then: cause 1 damage 
			user.NextDecision.HasPrompt( "Damage (1 remaining)" ).HasOptions( "E@1" ).Choose( "E@1" );
			//   And: wrap up
			user.NextDecision.HasPrompt( "Select location to Remove Blight OR Add Presence" ).HasOptions( "A5" ).Choose( "A5" );

		} );

		//  Then:
		space.Summary.ShouldBe( $"1D@1,2D@2,2E@1,2VLH" );

	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public async Task RavageBlight_Causes1Damage() {
		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[5]];

		// Given: 3 explorers, 3 presence
		space.InitDefault( Human.Explorer, 3 );
		spirit.Given_IsOn( space, 3 );

		//   And: Island Won't blight
		gameState.IslandWontBlight();

		//  When: they ravage and cause blight, destroying 1 presence
		Task task = space.Ravage();

		//  Then: destroyed presence causes 1 damage to explorer
		ApplyDamageToExplorers(spirit,task,1, space );

		await task.ShouldComplete();

		space.Summary.ShouldBe("1B,2E@1,2VLH");
	}

	//[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	//[Trait( "Targeting", "Range" )]
	//[Fact(Skip = "Upon further reflection, Volcan should not get the range boost as the defend is a residual effect of the power, not the power itself.")]
	//public async Task PeeksTower_ExtendsRangeFor_BargainsOfPowerAndProtection() {

	//	var (spirit, gameState, board) = Init();
	//	Space targetSpace = gameState.Tokens[board[5]];
	//	Space dahanSpace = gameState.Tokens[board[2]];

	//	// Given: Enough Presence to trigger Tower rule
	//	spirit.Given_IsOn( targetSpace, 5 );
	//	//   And: dahan in target & dahan space
	//	targetSpace.Given_HasTokens("2D@2");
	//	dahanSpace.Given_HasTokens("2D@2");

	//	//  When: they destroying presence via Powercard
	//	await spirit.When_ResolvingCard<BargainsOfPowerAndProtection>( (user) => {
	//		user.NextDecision.HasPrompt( "Bargains of Power and Protection: Target Space" ).Choose( "A5" );
	//		user.NextDecision.HasPrompt( "Select presence to remove from game." ).HasOptions( "VLH on A5" ).Choose( "VLH on A5" );
	//	} );

	//	//  Then: range-2 item has a defend.
	//	dahanSpace[Token.Defend].ShouldBe(2); // 1 per dahan
	//}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData(7, "A5,A6,A7,A8" )]        // Target space is not on Tower => normal range
	[InlineData( 8, "A1,A4,A5,A6,A7,A8" )] // Target space is on Tower => range boost
	public async Task PeeksTower_ExtendsRangeFor_UtterACurseOfDreadAndBone( int towerSpaceNum, string expectedRangeOptions) {
		var (spirit, gameState, board) = Init();
		Space towerSpace = gameState.Tokens[board[towerSpaceNum]];
		Space targetSpace = gameState.Tokens[board[8]];

		// Given: Enough Presence to trigger Tower rule
		spirit.Given_IsOn( towerSpace, 5 );
		//   And: enough elements to trigger threshold
		spirit.Configure().Elements("3 moon,2 animal");
		//   And: 1 blight in target land
		targetSpace.Init( Token.Blight, 1 );

		//  When: Utter a curse
		await spirit.When_ResolvingCard<UtterACurseOfDreadAndBone>( (user) => {
			user.Choose( targetSpace );
			user.NextDecision.HasPrompt( "Select Power Option" ).HasOptions( "Add Badland,Add Disease,Add Strife" ).Choose( "Add Disease" );

			user.AcceptsElementThreshold();

			//  Then: if the tower==target, then large range, else smaller range
			user.NextDecision.HasPrompt( "Add Disease" ).HasOptions( expectedRangeOptions ).ChooseFirst();
			user.NextDecision.HasPrompt( "Select land for 1 Damage" ).HasOptions( "A5,A6,A7" ).ChooseFirst(); // Adjacent, NOT Range
		} );
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData( 2, "A8" )]        // range-0
	[InlineData( 3, "A5,A6,A7,A8" )] // range-1
	public async Task PeeksTower_ExtendsRangeFor_UnleashATorrent( int presenceInTower, string expectedRangeOptions ) {
		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[8]];

		// Given: presence tower
		spirit.Given_IsOn( space, presenceInTower );
		//   And: 1 energy
		spirit.Energy=1;

		//  When: Unleash a Torrent
		await spirit.When_ResolvingCard<UnleashATorrentOfTheSelfsOwnEssence>( (user) => {
			user.NextDecision.HasPrompt( "Select action" )
				.HasOptions( "Gain 4 energy, Forget a Power Card to gain 4 more,Pay X Energy (min 1) to deal X Damage in a land at range 0" )
				.Choose( "Pay X Energy (min 1) to deal X Damage in a land at range 0" );
			user.NextDecision.HasPrompt( "Pay 1 energy/damage." ).HasOptions( "1,0" ).Choose( "1" );

			//  Then: able to target range-0 or range-1
			user.NextDecision.HasPrompt( "1 Damage" ).HasOptions( expectedRangeOptions ).ChooseFirst();
		} );
	}

	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // Move to Card-test file.
	[Trait( "Targeting", "Range" )]
	[Theory]
	[InlineData( 2, "A5,A6,A7,A8" )]       // range-1
	[InlineData( 3, "A1,A4,A5,A6,A7,A8" )] // range-2
	public async Task PeeksTower_ExtendsRangesFor_PerilsOfTheDeepIsland( int presenceInTower, string expectedRangeOptions ) {
		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[8]];

		// Given: presence tower
		spirit.Given_IsOn( space, presenceInTower );
		//   And: 1 energy
		spirit.Energy = 1;

		//  When: Perils of the Deepest Island
		await spirit.When_ResolvingCard<PerilsOfTheDeepestIsland>( (user) => {
			user.Choose( space );

			//  Then: range is adjusted for adding beasts
			user.NextDecision.HasPrompt( "Add beast" ).HasOptions( expectedRangeOptions ).ChooseFirst();
		} );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Fact]
	public async Task ExplosiveErruption_Level0_CausesDamage() {
		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[5]];

		// Given: 10 explorers 10 dahan, 4 presence
		space.Given_HasTokens("10E@1,10D@2");
		spirit.Given_IsOn( space, 4 );
		//   But: not enough elements to trigger Tier-1
		spirit.Configure().Elements( "0 fire,0 earth" );

		//  When: they trigger Explosive Erruption in target
		await spirit.When_ResolvingInnate(JaggedEarth.ExplosiveEruption.Name, (user) => {
			user.NextDecision.HasPrompt( "Explosive Eruption: Target Space" ).Choose( space );
			//   And: Destroy 2 presence
			user.NextDecision.HasPrompt( "# of presence to destroy?" ).HasOptions( "4,3,2,1,0" ).Choose( "2" );

			//  And: damage invaders in target
			ApplyDamageToExplorers( spirit, null, 2, space );
		} );
		//   Then: damages dahan INEFFICIENTLY
		space.Summary.ShouldBe( $"2D@1,8D@2,8E@1,2VLH" );
	}

	[Trait( "Special Rule", VolcanoLoomingHigh.CollapseInABlastOfLavaAndSteam )]
	[Trait( "Special Rule", VolcanicPeaksTowerOverTheLandscape.Name )] // does not Extend Explosive Erruption
	[Trait( "Token", "Badlands" )]
	[Theory]
	[InlineData( 0 )]
	[InlineData( 2 )]
	public async Task ExplosiveErruption_Level1_CausesDamage( int badlandsCount ) {
		int remaining = 10 - (2 + badlandsCount);

		var (spirit, gameState, board) = Init();
		Space space = gameState.Tokens[board[5]];
		Space adjacent = gameState.Tokens[board[7]];

		// Given: 10 explorers 10 dahan, 10 presence, n-badlands
		space.InitDefault( Human.Explorer, 10 );
		space.Dahan.Init( 10 );
		spirit.Given_IsOn( space, 10 );
		space.Init( Token.Badlands, badlandsCount );
		//   And: 10 exploreres, 10 dahan in adjacent
		adjacent.InitDefault( Human.Explorer, 10 );
		adjacent.Dahan.Init( 10 );
		adjacent.Init( Token.Badlands, badlandsCount );
		//   And: 2 fire and 2 earth
		spirit.Configure().Elements( "2 fire,2 earth" );

		//  When: they trigger Explosive Erruption in target
		await spirit.When_ResolvingInnate(JaggedEarth.ExplosiveEruption.Name, (user) => {
			user.NextDecision.HasPrompt( "Explosive Eruption: Target Space" ).Choose( space );
			//   And: Destroy 2 presence
			user.NextDecision.HasPrompt( "# of presence to destroy?" ).HasOptions( "10,9,8,7,6,5,4,3,2,1,0" ).Choose( "2" );
			//   And: damage invaders in target
			ApplyDamageToExplorers( spirit, null, 2 + badlandsCount, space );

			//  Then: VolcanicPeaksTowerOverTheLandscape does not extend range to A2 & A3
			user.NextDecision.HasPrompt( "Apply 2 damage to" ).HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( adjacent );
			//   And: damages invaders in adjacent
			ApplyDamageToExplorers( spirit, null, 2 + badlandsCount, adjacent );
		} );

		//  Then: dahan were damaged INEFFICIENTLY in target space
		space.Summary.ShouldBe( $"{2 + badlandsCount}D@1,{remaining}D@2,{remaining}E@1" + (badlandsCount == 0 ? "" : $",{badlandsCount}M")+",8VLH" );
		//   And: dahan were left alone in adjacent
		adjacent.Summary.ShouldBe( $"10D@2,{remaining}E@1" + (badlandsCount == 0 ? "" : $",{badlandsCount}M") );

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
		int remainingP = 12 - presenceDestroyed;

		var (spirit, gameState, board) = Init();
		Space targetSpace = gameState.Tokens[board[8]];
		Space adjPresence = gameState.Tokens[board[7]];
		Space adjBlight = gameState.Tokens[board[6]];

		// Given: enough elements to trigger all
		spirit.Configure().Elements("5 fire,3 air,5 earth");

		//   And: TargetSpace => 12 presence, 20 explorers
		spirit.Given_IsOn( targetSpace, 12 );
		targetSpace.InitDefault( Human.Explorer, 20 );

		//   And: AdjPresence => 1 presence, 20 explorers
		spirit.Given_IsOn( adjPresence );
		adjPresence.InitDefault( Human.Explorer, 20 );

		//   And: AdjBlight => 1 blight, 10 explorers
		adjBlight.Blight.Init(1);
		adjBlight.InitDefault( Human.Explorer, 20 );

		//  And: Island won't blight
		gameState.IslandWontBlight();

		// When: activate Innate
		await spirit.When_ResolvingInnate(JaggedEarth.ExplosiveEruption.Name, (user) => {
			user.NextDecision.HasOptions( "A7,A8" ).Choose( targetSpace );
			//  And: destroy presence
			user.NextDecision.HasPrompt( "# of presence to destroy?" ).HasOptions( "12,11,10,9,8,7,6,5,4,3,2,1,0" ).Choose( presenceDestroyed.ToString() );

			// Then: trigger Level 0
			if(1 <= presenceDestroyed) {
				ApplyDamageToExplorers( spirit, null, presenceDestroyed, targetSpace );
			}
			//  And: Level 1
			if(2 <= presenceDestroyed) {
				user.NextDecision.HasPrompt( $"Apply {presenceDestroyed} damage to" ).HasOptions( "A5,A6,A7,A8" ).Choose( adjBlight );
				ApplyDamageToExplorers( spirit, null, presenceDestroyed, adjBlight );
			}
			//  And: Level 2
			//  And: Level 3
			if(6 <= presenceDestroyed) {
				ApplyDamageToExplorers( spirit, null, 4, targetSpace ); // A8
				ApplyDamageToExplorers( spirit, null, 4, adjBlight );   // A6
				ApplyDamageToExplorers( spirit, null, 4, adjPresence ); // A7
			}
			if(10 <= presenceDestroyed) {
				// When: applying 4 more damage
				ApplyDamageToExplorers( spirit, null, 4, targetSpace ); // A8
				ApplyDamageToExplorers( spirit, null, 4, adjBlight );   // A6
				ApplyDamageToExplorers( spirit, null, 4, adjPresence ); // A7

				// When: Adding blight destroys presence and causes 1 more damage
				ApplyDamageToExplorers( spirit, null, 1, adjPresence ); // A7
			}

		} );

		//  And: Level 2
		int earnedFear = gameState.Fear.EarnedFear + gameState.Fear.ActivatedCards.Count * 4;
		earnedFear.ShouldBe( 4 <= presenceDestroyed ? presenceDestroyed : 0 );
		//  And: Level 3
		if(6 <= presenceDestroyed && presenceDestroyed < 10) {
			// target
			targetSpace.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4)}E@1,{remainingP}VLH" );
			// adj-blight
			adjBlight.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4)}E@1" );
			// adj-presence
			adjPresence.Summary.ShouldBe( $"{20 - 4}E@1,1VLH" );
		}
		if(10 <= presenceDestroyed) {
			// Then: target
			targetSpace.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4 + 4)}E@1,{remainingP}VLH" );
			// Then: adj-blight
			adjBlight.Summary.ShouldBe( $"1B,{20 - (presenceDestroyed + 4 + 4)}E@1" );
			// Then: adj-presence
			spirit.Presence.IsOn(adjPresence).ShouldBeFalse();
			adjPresence.Summary.ShouldBe( $"1B,{20 - (4 + 4 + 1)}E@1" );
		}

	}

	// 1Powered By the Furnace of the Earth

	#region private helpers

	// We need a special "When" method to use custom InnatePower 
	static Task When_ResolvingExplosiveErruption(Spirit spirit, Action<VirtualUser> userActions) {
		return spirit.When_ResolvingInnate(JaggedEarth.ExplosiveEruption.Name, userActions);
	}

	static void ApplyDamageToExplorers( Spirit spirit, Task task, int expectedDamage, Space onSpace ) {
		while(0 < expectedDamage) {
			string prompt = $"Damage ({expectedDamage} remaining)";
			task?.IsCompleted.ShouldBeFalse( nameof(ApplyDamageToExplorers) + "() => " + prompt );
			spirit.NextDecision()
				.HasPrompt( prompt )
				.IsForSpace( onSpace.SpaceSpec )
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
