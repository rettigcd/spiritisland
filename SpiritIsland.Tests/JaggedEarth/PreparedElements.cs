using static SpiritIsland.JaggedEarth.ObserveTheEverChangingWorld;

namespace SpiritIsland.Tests.JaggedEarth;

public class PreparedElements {

	[Fact]
	public void TwoStacksOnASpace() {
		var tokens = new CountDictionary<Token>();

		var el1 = new ElementToken();
		var el2 = new ElementToken();

		tokens[el1] = 1;
		tokens[el2] = 2;

		tokens[el1].ShouldBe(1);
		tokens[el2].ShouldBe(2);
	}

}

