using Shouldly;
using SpiritIsland.JaggedEarth;
using Xunit;

namespace SpiritIsland.Tests.JaggedEarth {
	public class StubbornSolidity_Tests {

		[Trait("Feature","Ravage")]
		[Fact]
		public void NoDahan_NoDefend() {
			var fxt = new GameFixture();

			// Given: a space to ravage on.
			var space = fxt.boardA[5];
			var tokens = fxt.gameState.Tokens[space];
			fxt.gameState.InvaderDeck.Ravage.Add( new InvaderCard( space.Terrain ) );

			//   And: no dahan
			tokens[TokenType.Dahan.Default] = 0;

			//   And: enough explorers to cause blight
			tokens[Invader.Explorer.Default] = 2;

			//  When: Card Played  (grow,select card,play card)
			//   And: Invader actions proceed
			Stone_Grows( fxt );
			BuysAndUses( fxt, StubbornSolidity.Name );

			//  Then: Blight
			tokens.Blight.Count.ShouldBe( 1 );
		}

		[Trait("Feature","Ravage")]
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		public void Dahan_Defends1Each_NoBlight(int dahanCount) {
			var fxt = new GameFixture();

			// Given: a space to ravage on.
			var space = fxt.boardA[5]; // a5
			var tokens = fxt.gameState.Tokens[space];
			fxt.gameState.InvaderDeck.Ravage.Add( new InvaderCard( space.Terrain ) );

			//   And: dahan in space
			tokens[TokenType.Dahan.Default] = dahanCount;

			//   And: enough explorers to cause blight
			tokens[Invader.Explorer.Default] = 2;

			//  When: Card Played  (grow,select card,play card)
			//   And: Invader actions proceed
			Stone_Grows( fxt );
			BuysAndUses( fxt, StubbornSolidity.Name );

			//  Then: No blight
			tokens.Blight.Count.ShouldBe( 0 );
		}

		[Trait("Feature","Ravage")]
		[Fact]
		public void LotsOfInvaders_DahanUnchanged() {
			var fxt = new GameFixture();

			// Given: a space to ravage on.
			var space = fxt.boardA[5]; // a5
			var tokens = fxt.gameState.Tokens[space];
			fxt.gameState.InvaderDeck.Ravage.Add( new InvaderCard( space.Terrain ) );

			//   And: dahan in space
			const int startingDahanCount = 10;
			tokens[TokenType.Dahan.Default] = startingDahanCount;

			//   And: 9 points of damage worth of invaders - would normally destroy 4 dahan and damage 1
			tokens[Invader.City.Default] = 3;

			//  When: Card Played  (grow,select card,play card)
			//   And: Invader actions proceed
			Stone_Grows( fxt );
			BuysAndUses( fxt, StubbornSolidity.Name );

			//  Then: all dahan still there
			tokens.Dahan[2].ShouldBe( startingDahanCount );


		}
		// Ravage should damage dahan, no change
		// Ravage should kill 1 dahahn, no change to dahan
		// Ravage should kill 2 dahan, dahan live and strike back.


		static void BuysAndUses( GameFixture fxt, string cardName ) {
			fxt.user.BuysPowerCard( cardName );
			fxt.user.SelectsFastAction( cardName );
			fxt.user.TargetsLand( "A1,A2,A3,A4,(A5),A6" );
		}

		static void Stone_Grows( GameFixture fxt ) {
			fxt.user.Growth_SelectsOption( "PlacePresence(2) / GainEnergy(3)" );
			fxt.user.Growth_PlacesPresence( "energy>A1;A2;A3;A4;(A5);A6;A7;A8" );
			fxt.user.Growth_GainsEnergy();
		}

	}

}
