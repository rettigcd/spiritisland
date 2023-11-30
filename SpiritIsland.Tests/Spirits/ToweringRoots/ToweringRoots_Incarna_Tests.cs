using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class ToweringRoots_Incarna_Tests : ToweringRoots_Base {

	public ToweringRoots_Incarna_Tests():base() {}

	[Fact]
	public async Task IncarnaProtectsDahanDuringRavage() {
		var tokens = _board[8].Tokens;
		// Given Dahan and town
		await tokens.Dahan.AddDefault(1);
		tokens.InitDefault(Human.Town,1);
		// Given Incarna, Vitality
		Given_IncarnaOn( tokens.Space );
		tokens.Init(Token.Vitality,1);

		// When ravage
		await tokens.Ravage();

		// Then: resulting tokens
		tokens.Summary.ShouldBe("1D@2,1T@2,1TRotJ-");

	}

	[Fact]
	public async Task DestroyingPresence_IncludesIncarnaToken_DoesntIncreaseDestroyedCount() {

		// Given: Presence on A2
		_board[2].Tokens.Init( _spirit.Presence.Token, 1 );
		//   And: Incarna on A3
		Given_IncarnaOn( _board[3] );

		// When: destroying presence (via Growth thru Sacrifice)
		await _spirit.When_ResolvingCard<GrowthThroughSacrifice>( u => {
			u.NextDecision.HasPrompt( "Select Presence to destroy" )
				.HasOptions( "TRotJ,TRotJ- on A3" )
				.Choose( "TRotJ- on A3" );

			// Clean up
			u.NextDecision.HasPrompt( "Select location to Remove Blight OR Add Presence" )
				.HasOptions( "A2" )
				.Choose( "A2" );

			u.NextDecision.HasPrompt( "Select Power Option" )
				.HasOptions( "Remove 1 blight from one of your lands,Add 1 presence to one of your lands" )
				.Choose( "Remove 1 blight from one of your lands" );

		} );

		// Then: Incarna is gone
		_gs.Tokens.ToString().ShouldBe( "Island-Mods:[none] A2:1TRotJ A3:[none]" );

		//  And: Destroyed Count is still 0
		_spirit.Presence.Destroyed.ShouldBe( 0 );
	}


	[Fact]
	public async Task MovePresence_IncludesIncarnaToken() {
		// Given: Presence on A2
		_board[2].Tokens.Init( _spirit.Presence.Token, 1 );
		//   And: Incarna on A3
		Given_IncarnaOn( _board[3] );

		// When: moving/pushing presence
		await _spirit.When_ResolvingCard<FlowLikeWaterReachLikeAir>( u => {
			u.NextDecision.HasPrompt( "Select Presence to push." )
				.HasOptions( "TRotJ,TRotJ- on A3,Done" )
				.Choose( "TRotJ- on A3" );

			// Clean up
			u.NextDecision.HasPrompt( "Push Presence to" )
				.HasOptions( "A2,A4" )
				.Choose( "A2" );
		} );

	}

	[Fact]
	public async Task IncarnaCountAsPresenceForSacredSite() {

		// Given: Presence & Incarna on A2
		_board[2].Tokens.Init( _spirit.Presence.Token, 1 );
		Given_IncarnaOn( _board[2] );

		// When: targetting from Sacred Site
		await _spirit.When_ResolvingCard<BloomingOfTheRocksAndTrees>( u => {

			// Then: Can select things range 1 from SS (
			u.NextDecision.HasPrompt( "Blooming of the Rocks and Trees: Target Space" )
				.HasOptions( "A1,A2,A3,A4,A5,A6" )
				.Choose( "A1" );

			// Cleanup
			u.NextDecision.HasPrompt( "Select action" )
				.HasOptions( "Add 1 Vitality.,Add 1 Wilds." )
				.Choose( "Add 1 Vitality." );
		} );
	}

	[Fact]
	public async Task PresencesSpacesIncludeIncarna() {
		// Given: Incarna on A2
		Given_IncarnaOn( _board[2] );

		// When: target from presence - range 0 (+1 for ToweringRoots Incarna)
		await _spirit.When_ResolvingCard<SapTheStrengthOfMultitudes>( u => {

			// Then: Can select incarna space + adjacents because Towering Roots +1 range for range 0
			u.NextDecision.HasPrompt( "Sap the Strength of Multitudes: Target Space" )
				.HasOptions( "A1,A2,A3,A4" )
				.Choose( "A2" );
		} );
	}

	[Fact]
	public void IsOn_DetectsIncarna() {
		var space = _board[2];

		// First not on island
		_presence.IsOn( space ).ShouldBeFalse();
		_presence.IsOn( _board ).ShouldBeFalse();
		_presence.IsOnIsland.ShouldBeFalse();

		// Then add to island
		Given_IncarnaOn( space );

		_presence.IsOn( space ).ShouldBeTrue();
		_presence.IsOn( _board ).ShouldBeTrue();
		_presence.IsOnIsland.ShouldBeTrue();

	}

	[Fact]
	public void TokensDeployedOn_IncludesIncarna() {
		// Given: Presence & Incarna on A2
		Space space = _board[2];
		space.Tokens.Init( _spirit.Presence.Token, 1 );
		Given_IncarnaOn( space );

		// When: query for what is there
		var tokens = _presence.TokensDeployedOn( space )
			.Select( x => x.Text )
			.OrderBy( x => x )
			.Join( "," );

		tokens.ShouldBe( "TRotJ,TRotJ-" );
	}

	[Fact]
	public async Task MoveIncarna_AsPartOfPlacingPresence() {
		_gs.MinorCards = new PowerCardDeck(
			new List<PowerCard>() {
				PowerCard.For<WeepForWhatIsLost>(),
				PowerCard.For<WeepForWhatIsLost>(),
				PowerCard.For<WeepForWhatIsLost>(),
				PowerCard.For<WeepForWhatIsLost>()
			},
			2,
			PowerType.Minor
		);

		Given_IncarnaOn( _board[2] );

		// When: placing presence
		await _spirit.When_Growing( 1, () => {
			var user = new VirtualUser( _spirit );
			// --- Draw Card ---
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();
			// --- Place Presence ---
			user.NextDecision.HasPrompt( "Select Growth to resolve" )
				.HasOptions( "PlacePresence(1),AddVitalityToIncarna" )
				.Choose( "PlacePresence(1)" );
			user.NextDecision.HasPrompt( "Select Presence to place" )
				.HasOptions( "2 energy,2 cardplay,Take Presence from Board" )
				.Choose( "Take Presence from Board" );
			user.NextDecision.HasPrompt( "Select Presence to place" )
				.HasOptions( "TRotJ-" )
				.Choose( "TRotJ-" );
			user.NextDecision.HasPrompt( "Where would you like to place your presence?" )
				.HasOptions( "A1,A2,A3,A4" )
				.Choose( "A4" );
			user.NextDecision.HasPrompt( "Select Growth to resolve" )
				.HasOptions( "AddVitalityToIncarna" )
				.Choose( "AddVitalityToIncarna" );
		} ).ShouldComplete();

		_board[4].Tokens.Summary.ShouldBe( "1TRotJ-,1V" );
		_gs.Tokens.ToVerboseString();

	}

	[Fact]
	public void SpaceDoestRepeat() {
		// Given: Presence & Incarna on A2
		_board[2].Tokens.Init( _spirit.Presence.Token, 1 );
		Given_IncarnaOn( _board[2] );

		_presence.Lands.Count().ShouldBe( 1 );
	}


}
