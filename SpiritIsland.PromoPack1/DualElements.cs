using System.Linq;

namespace SpiritIsland.PromoPack1 {
	public class DualElements : Track {
		public DualElements(params Element[] elements ):base( elements.Select(e=>e.ToString()).Join("") ) { 
			this.elements = elements;
		}
		public override void AddElement( CountDictionary<Element> elements ) {
			elements.AddRange(this.elements);
		}
		readonly Element[] elements;
	}

}
