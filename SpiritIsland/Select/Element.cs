using SpiritIsland.Select;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	namespace Select {
		public class Element : TypedDecision<ItemOption<SpiritIsland.Element>> {
			public Element(string prompt, IEnumerable<SpiritIsland.Element> elementOptions, Present present)
				:base(prompt, elementOptions.Select( x=>new ItemOption<SpiritIsland.Element>( x ) ), present ) { }
		}

	}
}