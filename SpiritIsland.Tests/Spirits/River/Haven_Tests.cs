namespace SpiritIsland.Tests.Spirits.River;

public class Haven_Tests {

	[Fact]
	public void ReplacesMassiveFlooding() {
		// Given: River
		var spirit = new RiverSurges();
		//  When: applying Haven asspect
		new Haven().ModSpirit(spirit);
		//  Then: 1 innate
		spirit.InnatePowers.Length.ShouldBe(1);
		//   And: it is Haven
		spirit.InnatePowers[0].Title.ShouldBe(CallToASunlitHaven.Name);
	}

	// !!! Test that we use the # on the Energy Track, not Energy adjusted by Bargain power cards.
}
