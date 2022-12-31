namespace SpiritIsland.Tests.Spirits.River;

public class RiverSurges_GrowthTests : GrowthTests {

	protected new VirtualRiverUser User;

	public RiverSurges_GrowthTests():base( new RiverSurges() ){
		User = new VirtualRiverUser( spirit );
	}

	#region growth

	[Fact]
	public void Reclaim_DrawCard_Energy() {

		// Given: using power pregression

		//   And: all cards played
		spirit.DiscardPile.AddRange( spirit.Hand );
		spirit.Hand.Clear();

		//  And: energy track is at 1
		Assert.Equal( 1, spirit.EnergyPerTurn );

		When_StartingGrowth();

		User.SelectsGrowthA_Reclaim();

		Assert_AllCardsAvailableToPlay( 5 );
		Assert_HasCardAvailable( "Drought" ); // gains 1st card drawn
		Assert_HasEnergy( 1 + 1 ); // 1 Growth energy + 1 from energy track

	}

	[Fact]
	public void TwoPresence() {
		// reclaim, +1 power card, +1 energy
		// +1 presense within 1, +1 presense range 1
		// +1 power card, +1 presense range 2

		Given_HasPresence( board[3] );
		spirit.Presence.Energy.Revealed.ShouldHaveSingleItem();

		When_StartingGrowth();
		User.SelectsGrowthB_2PP();

		Assert.Equal(2,spirit.EnergyPerTurn);
		Assert_HasEnergy( 2 ); // 2 from energy track
		spirit.Presence.Energy.Revealed.Count().ShouldBe(3); // # of spaces revealed, not energy per turn
	}

	[Fact]
	public void Power_Presence() {
		// +1 power card, 
		// +1 presense range 2

		spirit.Presence.Energy.Revealed.ShouldHaveSingleItem();
		Given_HasPresence( board[3] );

		When_StartingGrowth();

		User.SelectsGrowthC_Draw_Energy();

		Assert_HasCardAvailable( "Drought" );
		Assert_HasEnergy( 1 ); // didn't increase energy track.
		spirit.Presence.Energy.Revealed.Count().ShouldBe(1);
		spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
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
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
		var fix = new ConfigurableTestFixture { Spirit = new RiverSurges() };
		await fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, "" );
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
	public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ) {
		var fix = new ConfigurableTestFixture { Spirit = new RiverSurges() };
		await fix.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
		fix.VerifyReclaim1Count( canReclaim1? 1 : 0 );
	}

	#endregion

	#region Buying Cards

	//	Boon of Vigor => 0 => fast,any spirit		=> sun, water, plant	=> If you target yourself, gain 1 energy.  If you target another spirit, they gain 1 energy per power card they played this turn
	//	River's Bounty => 0 => slow, range 0, any	=> sun, water, animal	=> gather up to 2 dahan.  If ther are now at least 2 dahan, add 1 dahan and gain +1 energy
	//	Wash Away => 1 => slow, range 1, any		=> water mountain		=> Push up to 3 explorers / towns
	//	Flash Floods => 2 => fast, range 1, any		=> sun, water			=> 1 Damange.  If target land is costal +1 damage.

	[Theory]
	[InlineData("Boon of Vigor")]
	[InlineData("River's Bounty")]
	[InlineData("Wash Away")]
	[InlineData("Flash Floods")]
	public void SufficientEnergyToBuy(string cardName) {

		var card = FindSpiritsAvailableCard( cardName );
		spirit.Energy = card.Cost;

		// When:
		PlayCard( card );

		// Then: card is in Active/play list
		Assert.Contains( spirit.InPlay, c => c == card );

		//  And: card is not in Available list
		Assert.DoesNotContain( spirit.Hand, c => c == card );

		Assert_CardInActionListIf( card );

		// Assert_InnateInActionListIf(Speed.Fast); // we now put all actions in the list at the same time

		// Money is spent
		Assert.Equal( 0, spirit.Energy );

	}

	protected void Assert_CardInActionListIf(PowerCard card) {

		var unresolvedCards = spirit.GetAvailableActions(card.DisplaySpeed)
			.OfType<PowerCard>()
			.ToArray();

		//  And: card is in Unresolved Action list
		Assert.Contains(card, unresolvedCards);
	}

	[Theory]
	[InlineData("Wash Away")]
	[InlineData("Flash Floods")]
	public void InsufficientEnergyToBuy(string cardName){
		var card = spirit.Hand.VerboseSingle(c=>c.Name == cardName);
		spirit.Energy = card.Cost - 1;

		// When:
		void Purchase() => PlayCard( card );

		Assert.Throws<InsufficientEnergyException>( Purchase );
	}

	[Fact]
	public void CantActivateDiscardedCards() {
		// Given: Boon of vigor already played
		PowerCard card = FindSpiritsAvailableCard("Boon of Vigor");
		Discard(card);

		// When
		void Purchase() => PlayCard( card );

		Assert.Throws<CardNotAvailableException>( Purchase );
	}

	void Discard(PowerCard card) {
		spirit.Hand.Remove(card);
		spirit.DiscardPile.Add(card);
	}

	PowerCard FindSpiritsAvailableCard(string cardName) => spirit.Hand.VerboseSingle(c => c.Name == cardName);

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
			"A" => BoardA,
			"B" => BoardB,
			"C" => BoardC,
			"D" => BoardD,
			_ => null,
		};
		_gameState = new GameState( river, board );
		river.InitSpirit(board,_gameState);
		Assert.Equal(expectedStartingSpaces,new ReadOnlyBoundPresence( river, _gameState ).Spaces.Select(s=>s.Label).Join(","));
	}

	#endregion

}

public class RiverSurges_GrowthTests2 : RiverGame {

	public RiverSurges_GrowthTests2(){
		var gs = new GameState( spirit, Board.BuildBoardA() );
		gs.Initialize();
		game = new SinglePlayer.SinglePlayerGame(gs);
	}

 	[Fact]
	public void Reclaim1_TriggersImmediately(){
		// pull card track 2 * 2 = triggers reclaim 

		// !!! I think there is an await missing between SelectsGrowthOptions(1) and the 1st place-precense
		// which causes the thread to return before the engine has queued up the PlacePresence decision
		// Problem only appears in things that use RiverGame base class.

		User.SelectsGrowthB_2PP( "cardplays>A5", "cardplays>A5" );

		User.PlaysCard( WashAway.Name );
		User.PlaysCard( RiversBounty.Name );

		game.Spirit.Energy++; // pretend we played Rivers Bounty and gained 1 energy
		User.IsDoneWith(Phase.Slow);

		User.SelectsGrowthB_2PP("cardplays>A5","cardplays>A5");

		// Can reclaim River's Bounty
		User.Reclaims1FromTrackBonus( "Wash Away $1 (Slow),[River's Bounty $0 (Slow)]" );

		// Can buy all 3 of River's cards including Bounty
		game.Spirit.Energy.ShouldBe(2,"need 2 energy to purcahse 0+0+2 cards");

		User.PlaysCard( RiversBounty.Name ); // 0
		User.PlaysCard( BoonOfVigor.Name );  // 0
		User.PlaysCard( FlashFloods.Name );  // 2
		User.IsDoneWith( Phase.Fast );

	}

}