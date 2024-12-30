using SpiritIsland.Maui;

namespace SpiritIsland.Tests.ViewModels; 

public class ElementPanel_Tests {

	[Fact]
	public void FindElementOptionsInDictionary() {
		Dictionary<IOption, List<IOptionModel>> lookup = []; // find controls
		var fire = new ItemOption<Element>(Element.Fire);
		lookup.Add(fire, []);
		lookup.ContainsKey(fire).ShouldBeTrue();

		IOption fireOption = new ItemOption<Element>(Element.Fire);
		lookup.ContainsKey(fireOption).ShouldBeTrue();

	}

}
