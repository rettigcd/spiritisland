namespace SpiritIsland.Tests.Spirits.River;

public class Sunshine_Tests {
	[Fact]
	public void InitSpirit() {
		// Given: River
		var spirit = new RiverSurges();

		//  When: applying Haven asspect
		new Sunshine().ModSpirit(spirit);

		//  Then: Boon of Vigor is removed
		spirit.Hand.Any(c=>c.Title==BoonOfVigor.Name).ShouldBeFalse();

		//   And: 2 innates
		spirit.InnatePowers.Length.ShouldBe(2);
		//   And: 2nd is Sunshine
		spirit.InnatePowers[1].Title.ShouldBe(BoonOfSunshinesPromise.Name);

		//   And: gained in 1 energy
		spirit.Energy.ShouldBe(1);

	}

	// !!! Test that we use the # on the Energy Track, not Energy adjusted by Bargain power cards.
}
