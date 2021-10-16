using System.Linq;

namespace SpiritIsland {

	public class DualElements : Track {
		public DualElements(params Element[] elements ):base( elements.Select(e=>e.ToString()).Join("")+" energy" ) { // + energy because it is drawing in the energy track
			this.elements = elements;
		}
		public override void AddElement( CountDictionary<Element> elements ) {
			elements.AddRange(this.elements);
		}
		readonly Element[] elements;
	}

}
