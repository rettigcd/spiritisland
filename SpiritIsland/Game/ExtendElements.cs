using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland
{
	static public class ExtendElements {

		static public bool Has(this CountDictionary<Element> activated, Dictionary<Element, int> needed ) {
			return needed.All( pair => pair.Value <= activated[pair.Key] );
		}

		static public bool Has( this CountDictionary<Element> activated, params Element[] requiredElements ) {
			var required = requiredElements
				.GroupBy( x => x )
				.ToDictionary( grp => grp.Key, grp => grp.Count() );
			return activated.Has( required );
		}

	}

}