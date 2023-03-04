namespace SpiritIsland.Tests;

[Trait("Token","Badlands")]
public class Badlands_Tests {

	[Trait("Invaders","Ravage")]
	[Fact]
	public void NoInvaderDamageToDahan_NoBadlandDamageToDahan() {
		var fxt = new GameFixture()
			.Start();

		// Given: a space to ravage on.
		var space = fxt.board[5];
		var tokens = fxt.gameState.Tokens[space];
		fxt.InitRavageCard( space );

		// And: disable ravage on A2 which has the same space
		fxt.gameState.Tokens[fxt.board[2]].SkipRavage("test");

		//  And: 1 bad lands, 1 explorer, 1 dahan, 1 defend
		tokens.Badlands.Init(1);
		tokens.InitDefault( Human.Explorer, 1 );
		tokens.InitDefault( Human.Dahan, 1 );
		tokens.Beasts.Init( 0 );
		tokens.Defend.Add(1);
		tokens.Summary.ShouldBe("1D@2,1E@1,1G,1M,2TS");

		// When: Grow, Skip Buy, then Ravage
		fxt.user.Growth_SelectAction( "ReclaimAll" );
		fxt.user.IsDoneBuyingCards();

		//  Then: ravage happened
		fxt.user.WaitForNext();
		fxt.gameState.RoundNumber.ShouldBe(2);
		fxt.ravages.Count.ShouldBe(1);
		var ravage = fxt.ravages[0];

		tokens.Dahan.CountAll.ShouldBe(1);

		//   And: no damage applied to dahan
		ravage.defenderDamageFromAttackers.ShouldBe(0);
		ravage.dahanDestroyed.ShouldBe(0);
		ravage.startingDefenders.Total.ShouldBe(1);

	}

	[Trait("Invaders","Ravage")]
	[Fact]
	public void InvaderDamageToDahan_BadlandDamageToDahan() {
		var fxt = new GameFixture().Start();

		// Given: a space to ravage on.
		var space = fxt.board[5];
		var tokens = fxt.gameState.Tokens[space];
		fxt.InitRavageCard( space );

		// And: disable ravage on A2 which has the same space
		fxt.gameState.Tokens[fxt.board[2]].SkipRavage("test");

		//  And: 1 bad lands, 1 explorer, 1 dahan
		tokens.Badlands.Init(1);
		tokens.InitDefault( Human.Explorer, 1);
		tokens.InitDefault(Human.Dahan, 1);
		tokens.Beasts.Init(0);
		tokens.Summary.ShouldBe( "1D@2,1E@1,1M,2TS" );

		// When: Grow, Skip Buy, then Ravage
		fxt.user.Growth_SelectAction( "ReclaimAll" );
		fxt.user.IsDoneBuyingCards();

		//  Then: ravage happened
		fxt.user.WaitForNext();
		fxt.gameState.RoundNumber.ShouldBe(2);
		fxt.ravages.Count.ShouldBe(1);
		var ravage = fxt.ravages[0];

		//   And: 1 explorer damage
		ravage.defenderDamageFromAttackers.ShouldBe(1);

		//   and: dahan destroyed,  1 explorer + 1 badland damage = 2 damage, destroying 1 dahan
		tokens.Dahan.CountAll.ShouldBe(0);
		ravage.dahanDestroyed.ShouldBe(1);

	}

}
