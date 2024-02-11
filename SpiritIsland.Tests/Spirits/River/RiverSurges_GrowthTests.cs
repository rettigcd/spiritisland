namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class RiverSurges_GrowthTests : BoardAGame {


	public RiverSurges_GrowthTests():base( new RiverSurges() ){}

	#region growth

	[Fact]
	public async Task Reclaim_DrawCard_Energy() {

		// Given: using power pregression

		//   And: all cards played
		_spirit.DiscardPile.AddRange( _spirit.Hand );
		_spirit.Hand.Clear();

		//  And: energy track is at 1
		Assert.Equal( 1, _spirit.EnergyPerTurn );

		await _spirit.When_Growing( (user) => {
			user.SelectsGrowthA_Reclaim();
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 5 );
		_spirit.Assert_HasCardAvailable( "Drought" ); // gains 1st card drawn
		_spirit.Assert_HasEnergy( 1 + 1 ); // 1 Growth energy + 1 from energy track

	}

	[Fact]
	public async Task TwoPresence() {
		// reclaim, +1 power card, +1 energy
		// +1 presense within 1, +1 presense range 1
		// +1 power card, +1 presense range 2

		_spirit.Given_IsOn( _board[3] );
		_spirit.Presence.Energy.Revealed.ShouldHaveSingleItem();

		await _spirit.When_Growing( (user) => {
			user.SelectsGrowthB_2PP();
		} );

		Assert.Equal(2,_spirit.EnergyPerTurn);
		_spirit.Assert_HasEnergy( 2 ); // 2 from energy track
		_spirit.Presence.Energy.Revealed.Count().ShouldBe(3); // # of spaces revealed, not energy per turn
	}

	[Fact]
	public async Task Power_Presence() {
		// +1 power card, 
		// +1 presense range 2

		_spirit.Presence.Energy.Revealed.ShouldHaveSingleItem();
		_spirit.Given_IsOn( _board[3] );

		await _spirit.When_Growing( (user) => {
			user.SelectsGrowthC_Draw_Energy();
		} );

		_spirit.Assert_HasCardAvailable( "Drought" );
		_spirit.Assert_HasEnergy( 1 ); // didn't increase energy track.
		_spirit.Presence.Energy.Revealed.Count().ShouldBe(1);
		_spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
	}

	#endregion

	#region presence tracks

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,2)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,3)]
	[InlineDataAttribute(5,4)]
	[InlineDataAttribute(6,4)]
	[InlineDataAttribute(7,5)]
	public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ) {
		var fix = new ConfigurableTestFixture { Spirit = new RiverSurges() };
		fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, "" );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1,false)]
	[InlineDataAttribute(2,2,false)]
	[InlineDataAttribute(3,2,false)]
	[InlineDataAttribute(4,3,false)]
	[InlineDataAttribute(5,3,true)]
	[InlineDataAttribute(6,4,true)]
	[InlineDataAttribute(7,5,true)]
	public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ) {
		var fix = new ConfigurableTestFixture { Spirit = new RiverSurges() };
		fix.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
		fix.VerifyReclaim1Count( canReclaim1? 1 : 0 );
	}

	#endregion

	#region Buying Cards

	//	Boon of Vigor => 0 => fast,any spirit		=> sun, water, plant	=> If you target yourself, gain 1 energy.  If you target another spirit, they gain 1 energy per power card they played this turn
	//	River's Bounty => 0 => slow, range 0, any	=> sun, water, animal	=> gather up to 2 dahan.  If ther are now at least 2 dahan, add 1 dahan and gain +1 energy
	//	Wash Away => 1 => slow, range 1, any		=> water mountain		=> Push up to 3 explorers / towns
	//	Flash Floods => 2 => fast, range 1, any		=> sun, water			=> 1 Damange.  If target land is coastal +1 damage.

	[Theory]
	[InlineData("Boon of Vigor")]
	[InlineData("River's Bounty")]
	[InlineData("Wash Away")]
	[InlineData("Flash Floods")]
	public void SufficientEnergyToBuy(string cardName) {

		var card = FindSpiritsAvailableCard( cardName );
		_spirit.Energy = card.Cost;

		// When:
		_spirit.When_PlayingCards( card );

		// Then: card is in Active/play list
		Assert.Contains( _spirit.InPlay, c => c == card );

		//  And: card is not in Available list
		Assert.DoesNotContain( _spirit.Hand, c => c == card );

		Assert_CardInActionListIf( card );

		// Assert_InnateInActionListIf(Speed.Fast); // we now put all actions in the list at the same time

		// Money is spent
		Assert.Equal( 0, _spirit.Energy );

	}

	protected void Assert_CardInActionListIf(PowerCard card) {

		var unresolvedCards = _spirit.GetAvailableActions(card.DisplaySpeed)
			.OfType<PowerCard>()
			.ToArray();

		//  And: card is in Unresolved Action list
		Assert.Contains(card, unresolvedCards);
	}

	[Theory]
	[InlineData("Wash Away")]
	[InlineData("Flash Floods")]
	public void InsufficientEnergyToBuy(string cardName){
		var card = _spirit.Hand.VerboseSingle(c=>c.Name == cardName);
		_spirit.Energy = card.Cost - 1;

		// When:
		void Purchase() => _spirit.When_PlayingCards( card );

		Assert.Throws<InsufficientEnergyException>( Purchase );
	}

	[Fact]
	public void CantActivateDiscardedCards() {
		// Given: Boon of vigor already played
		PowerCard card = FindSpiritsAvailableCard("Boon of Vigor");
		Discard(card);

		// When
		void Purchase() => _spirit.When_PlayingCards( card );

		Assert.Throws<CardNotAvailableException>( Purchase );
	}

	void Discard(PowerCard card) {
		_spirit.Hand.Remove(card);
		_spirit.DiscardPile.Add(card);
	}

	PowerCard FindSpiritsAvailableCard(string cardName) => _spirit.Hand.VerboseSingle(c => c.Name == cardName);

	#endregion

	#region Initial Presence Placing

	[Theory]
	[InlineData("A5")]
	[InlineData("B6")]
	[InlineData("C8")]
	[InlineData("D3")]
	public void StartsOnHighestNumberedWetlands(string expectedStartingSpaces){
		var river = new RiverSurges();
		var board = expectedStartingSpaces[..1] switch {
			"A" => Board.BuildBoardA( GameBuilder.FourBoardLayout[0] ),
			"B" => Board.BuildBoardB( GameBuilder.FourBoardLayout[1] ),
			"C" => Board.BuildBoardC( GameBuilder.FourBoardLayout[2] ),
			"D" => Board.BuildBoardD( GameBuilder.FourBoardLayout[3] ),
			_ => null,
		};
		_gameState = new GameState( river, board );
		river.InitSpirit(board,_gameState);
		Assert.Equal(expectedStartingSpaces,river.Presence.Lands.Tokens().SelectLabels().Join(","));
	}

	#endregion

}

public class RiverSurges_GrowthTests2 {

 	[Fact]
	public void Reclaim1_TriggersImmediately(){

		var spirit = new RiverSurges();
		
		var user = new VirtualUser( spirit );

		var gs = new GameState( spirit, Board.BuildBoardA() );
		gs.Initialize();
		new SinglePlayer.SinglePlayerGame(gs).Start();

		// pull card track 2 * 2 = triggers reclaim 

		user.SelectsGrowthB_2PP( "cardplays>A5", "cardplays>A5" );

		user.PlaysCard( WashAway.Name );
		user.PlaysCard( RiversBounty.Name );

		spirit.Energy++; // pretend we played Rivers Bounty and gained 1 energy
		user.IsDoneWith(Phase.Slow);

		user.SelectsGrowthB_2PP("cardplays>A5","cardplays>A5");

		// Can reclaim River's Bounty
		user.Reclaims1FromTrackBonus( "Wash Away $1 (Slow),[River's Bounty $0 (Slow)]" );

		// Can buy all 3 of River's cards including Bounty
		spirit.Energy.ShouldBe(2,"need 2 energy to purcahse 0+0+2 cards");

		user.PlaysCard( RiversBounty.Name ); // 0
		user.PlaysCard( BoonOfVigor.Name );  // 0
		user.PlaysCard( FlashFloods.Name );  // 2
		user.IsDoneWith( Phase.Fast );

	}

}