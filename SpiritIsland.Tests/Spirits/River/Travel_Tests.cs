namespace SpiritIsland.Tests.Spirits.River;

public class Travel_Tests {

	[Fact]
	public void InitSpirit() {
		// Given: River
		var spirit = new RiverSurges();

		//  When: applying travel
		new Travel().ModSpirit(spirit);

		//  Then: Remove 'Rivers Domain' rule

		//   And: Add 'Travel on the River's Back' rule

		//   And: Add 'People Tend to the River, River Tends to the People'

	}

}