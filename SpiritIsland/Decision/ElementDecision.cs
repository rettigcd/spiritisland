using SpiritIsland.Decision;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	namespace Decision {

		public class ElementDecision : TypedDecision<ItemOption<Element>> {
			public ElementDecision(string prompt, IEnumerable<Element> elementOptions, Present present)
				:base(prompt, elementOptions.Select(x=>new ItemOption<Element>(x)), present ) { }
		}

	}
}