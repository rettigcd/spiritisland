using Shouldly;
using Xunit;

namespace SpiritIsland.Tests {
	public class Badlands_Tests {

		[Trait("Feature","Ravage")]
		[Fact]
		public void NoInvaderDamageToDahan_NoBadlandDamageToDahan() {
			var fxt = new GameFixture()
				.Start();

			// Given: a space to ravage on.
			var space = fxt.board[5];
			var tokens = fxt.gameState.Tokens[space];
			fxt.gameState.InvaderDeck.Ravage.Add( new InvaderCard( space.Terrain ) );

			// And: disable ravage on A2 which has the same space
			fxt.gameState.SkipRavage(fxt.board[2]);

			//  And: 1 bad lands, 1 explorer, 1 dahan, 1 defend
			tokens.Badlands.Init(1);
			tokens[Invader.Explorer.Default] = 1;
			tokens[TokenType.Dahan.Default] = 1;
			tokens.Defend.Add(1);
			tokens.Summary.ShouldBe("1D@2,1E@1,1G@1,1M@1");

			// When: Grow, Skip Buy, then Ravage
			fxt.user.Growth_SelectsOption( "ReclaimAll" );
			fxt.user.IsDoneBuyingCards();

			//  Then: ravage happened
			fxt.gameState.RoundNumber.ShouldBe(2);
			fxt.ravages.Count.ShouldBe(1);
			var ravage = fxt.ravages[0];

			tokens.Dahan.Count.ShouldBe(1);

			//   And: no damage applied to dahan
			ravage.dahanDamageFromInvaders.ShouldBe(0);
			ravage.dahanDestroyed.ShouldBe(0);
			ravage.startingDahan.ShouldBe(1);

		}

		[Trait("Feature","Ravage")]
		[Fact]
		public void InvaderDamageToDahan_BadlandDamageToDahan() {
			var fxt = new GameFixture()
				.Start();

			// Given: a space to ravage on.
			var space = fxt.board[5];
			var tokens = fxt.gameState.Tokens[space];
			fxt.gameState.InvaderDeck.Ravage.Add( new InvaderCard( space.Terrain ) );

			// And: disable ravage on A2 which has the same space
			fxt.gameState.SkipRavage(fxt.board[2]);

			//  And: 1 bad lands, 1 explorer, 1 dahan
			tokens.Badlands.Init(1);
			tokens[Invader.Explorer.Default] = 1;
			tokens[TokenType.Dahan.Default] = 1;
			tokens.Summary.ShouldBe("1D@2,1E@1,1M@1");

			// When: Grow, Skip Buy, then Ravage
			fxt.user.Growth_SelectsOption( "ReclaimAll" );
			fxt.user.IsDoneBuyingCards();

			//  Then: ravage happened
			fxt.gameState.RoundNumber.ShouldBe(2);
			fxt.ravages.Count.ShouldBe(1);
			var ravage = fxt.ravages[0];

			//   And: 1 explorer damage
			ravage.dahanDamageFromInvaders.ShouldBe(1);

			//   and: dahan destroyed,  1 explorer + 1 badland damage = 2 damage, destroying 1 dahan
			tokens.Dahan.Count.ShouldBe(0);
			ravage.dahanDestroyed.ShouldBe(1);

		}


	}
}
