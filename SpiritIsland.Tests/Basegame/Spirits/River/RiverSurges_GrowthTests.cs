using Shouldly;
using SpiritIsland;
using SpiritIsland.Basegame;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class RiverSurges_GrowthTests : GrowthTests {

		protected new VirtualRiverUser User;

		public RiverSurges_GrowthTests():base( new RiverSurges().UsePowerProgression() ){
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
			Assert_HasCardAvailable( "Uncanny Melting" ); // gains 1st card in power progression
			Assert_HasEnergy( 1 + 1 ); // 1 Growth energy + 1 from energy track

		}

		[Fact]
		public void TwoPresence() {
			// reclaim, +1 power card, +1 energy
			// +1 presense withing 1, +1 presense range 1
			// +1 power card, +1 presense range 2

			Given_HasPresence( board[3] );
			Assert.Equal(1,spirit.Presence.Energy.RevealedCount);

			When_StartingGrowth();
			User.SelectsGrowthB_2PP();

			Assert.Equal(2,spirit.EnergyPerTurn);
			Assert_HasEnergy( 2 ); // 2 from energy track
			Assert.Equal(3,spirit.Presence.Energy.RevealedCount); // # of spaces revealed, not energy per turn
		}

		[Fact]
		public void Power_Presence() {
			// +1 power card, 
			// +1 presense range 2

			Assert.Equal(1,spirit.Presence.Energy.RevealedCount);
			Given_HasPresence( board[3] );

			When_StartingGrowth();

			User.SelectsGrowthC_Draw_Energy();

			Assert_HasCardAvailable( "Uncanny Melting" ); // gains 1st card in power progression
			Assert_HasEnergy( 1 ); // didn't increase energy track.
			Assert.Equal(1,spirit.Presence.Energy.RevealedCount);
			Assert.Equal(2,spirit.Presence.CardPlays.RevealedCount);
		}

		#endregion

		#region presence tracks

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,4)]
		[InlineDataAttribute(7,5)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
			// energy:	1 2 2 3 4 4 5

			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre(expectedEnergyGrowth,1);
		}

		[Theory]
		[InlineDataAttribute(1,1,false)]
		[InlineDataAttribute(2,2,false)]
		[InlineDataAttribute(3,2,false)]
		[InlineDataAttribute(4,3,false)]
		[InlineDataAttribute(5,3,true)]
		[InlineDataAttribute(6,4,true)]
		[InlineDataAttribute(7,5,true)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ){
			// cards:	1 2 2 3 reclaim-1 4 5

			Given_HasPresence( board[3] );
			Given_HalfOfPowercardsPlayed();

			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			When_StartingGrowth();

			User.SelectsGrowthC_Draw_Energy( "energy>A1;A2;A3;A4;A5" );

			if(canReclaim1)
				User.Reclaims1CardIfAny();
		}

		#endregion

		[Theory]
		[InlineData(1,"Uncanny Melting")]
		[InlineData(2,"Nature's Resilience")]
		[InlineData(3,"Pull Beneath the Hungry Earth")]
		[InlineData(4,"Accelerated Rot")]
		[InlineData(5,"Song of Sanctity")]
		[InlineData(6,"Tsunami")]
		[InlineData(7,"Encompassing Ward")]
		public void PowerProgressionCards( int count, string lastPowerCard ){
			var drawPowerCard = new DrawPowerCard();
			var ctx = new SpiritGameStateCtx( spirit, gameState, Cause.Growth );
			while(count-- > 0)
				_ = drawPowerCard.ActivateAsync( ctx );

			Assert_HasCardAvailable( lastPowerCard );
		}

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

			var card = FindSpiritsAvailableCard(cardName);
			spirit.Energy = card.Cost;

			// When:
			spirit.PurchaseAvailableCards(card);

			// Then: card is in Active/play list
			Assert.Contains(spirit.PurchasedCards, c => c == card);

			//  And: card is not in Available list
			Assert.DoesNotContain(spirit.Hand, c => c == card);

			Assert_CardInActionListIf(card);

			// Assert_InnateInActionListIf(Speed.Fast); // we now put all actions in the list at the same time

			// Money is spent
			Assert.Equal(0, spirit.Energy);

		}

		protected void Assert_CardInActionListIf(PowerCard card) {

			var unresolvedCards = spirit.GetAvailableActions(card.Speed)
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
			void Purchase() => spirit.PurchaseAvailableCards( card );

			Assert.Throws<InsufficientEnergyException>( Purchase );
		}

		[Fact]
		public void InsufficientCardCountToBuy(){
			
			// Given: has 2 cards they want to play
			var card1 = spirit.Hand[0];
			var card2 = spirit.Hand[1];

			//  And: lots of energy
			spirit.Energy = 200;

			//  But: can only play 1 card
			Assert.Equal(1,spirit.NumberOfCardsPlayablePerTurn);

			// When:
			void Purchase() => spirit.PurchaseAvailableCards( card1, card2 );

			Assert.Throws<InsufficientCardPlaysException>(Purchase);
		}

		[Fact]
		public void CantActivateDiscardedCards() {
			// Given: Boon of vigor already played
			PowerCard card = FindSpiritsAvailableCard("Boon of Vigor");
			Discard(card);

			// When
			void Purchase() => spirit.PurchaseAvailableCards( card );

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
			var board = expectedStartingSpaces.Substring(0,1) switch {
				"A" => BoardA,
				"B" => BoardB,
				"C" => BoardC,
				"D" => BoardD,
				_ => null,
			};
			river.Initialize(board,new GameState(river));
			Assert.Equal(expectedStartingSpaces,river.Presence.Spaces.Select(s=>s.Label).Join(","));
		}

		#endregion

	}

	public class RiverSurges_GrowthTests2 : RiverGame {

		public RiverSurges_GrowthTests2(){
			var gs = new GameState( spirit, Board.BuildBoardA() );
			game = new SinglePlayer.SinglePlayerGame(gs);
		}

 		[Fact]
		public void Reclaim1_TriggersImmediately(){
			// pull card track 2 * 2 = triggers reclaim 

			// !!! I think there is an await missing between SelectsGrowthOptions(1) and the 1st place-precense
			// which causes the thread to return before the engine has queued up the PlacePresence decision
			// Problem only appears in things that use RiverGame base class.

			User.SelectsGrowthB_2PP( "cardplays>A5", "cardplays>A5" );

			User.BuysPowerCard( WashAway.Name );
			User.BuysPowerCard( RiversBounty.Name );

			game.Spirit.Energy++; // pretend we played Rivers Bounty and gained 1 energy
			User.IsDoneWith(Speed.Slow);

			User.SelectsGrowthB_2PP("cardplays>A5","cardplays>A5");

			// Can reclaim River's Bounty
			User.Reclaims1FromTrackBonus( "Wash Away $1 (Slow),{River's Bounty $0 (Slow)}" );

			// Can buy all 3 of River's cards including Bounty
			game.Spirit.Energy.ShouldBe(2,"need 2 energy to purcahse 0+0+2 cards");

			User.BuysPowerCard( RiversBounty.Name ); // 0
			User.BuysPowerCard( BoonOfVigor.Name );  // 0
			User.BuysPowerCard( FlashFloods.Name );  // 2
			User.IsDoneWith( Speed.Fast );

		}

	}

}
