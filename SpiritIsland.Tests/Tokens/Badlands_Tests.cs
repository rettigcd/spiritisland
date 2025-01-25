namespace SpiritIsland.Tests;

[Trait("Token","Badlands")]
public class Badlands_Tests {

	[Trait("Invaders","Ravage")]
	[Fact]
	public async Task NoInvaderDamageToDahan_NoBadlandDamageToDahan() {

		var gs = new SoloGameState();

		// Given a space to ravage on.
		var space = gs.Board[7].ScopeSpace;

		//  And: 1 bad lands, 1 explorer, 1 dahan, 1 defend
		space.Badlands.Init(1);
		space.InitDefault(Human.Explorer, 1);
		space.InitDefault(Human.Dahan, 1);
		space.Beasts.Init(0);
		space.Defend.Add(1);
		space.Summary.ShouldBe("1D@2,1E@1,1G,1M");

		// When: ravage
		await space.Ravage();

		// Then: no damage to dahan
		space.Summary.ShouldBe("1D@2,1G,1M");
	}

	[Trait("Invaders","Ravage")]
	[Fact]
	public async Task InvaderDamageToDahan_BadlandDamageToDahan() {

		var gs = new SoloGameState();

		// Given: a space to ravage on.
		var space = gs.Board[7].ScopeSpace;

		//  And: 1 bad lands, 1 explorer, 1 dahan
		space.Badlands.Init(1);
		space.InitDefault(Human.Explorer, 1);
		space.InitDefault(Human.Dahan, 1);
		space.Beasts.Init(0);
		space.Summary.ShouldBe("1D@2,1E@1,1M");

		// When: ravage
		await space.Ravage();

		// Then: 1 explorer damage
		//   and: dahan destroyed,  1 explorer + 1 badland damage = 2 damage, destroying 1 dahan
		space.Summary.ShouldBe("1E@1,1M");
	}

}
