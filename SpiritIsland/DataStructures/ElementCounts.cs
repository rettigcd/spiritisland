using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class ElementCounts : CountDictionary<Element> {

		#region constructors
		public ElementCounts(){ }

		public ElementCounts(IEnumerable<Element> items):base(items){ }

		public ElementCounts(Dictionary<Element,int> inner):base(inner){ }

		#endregion

		public new ElementCounts Clone() {
			var clone = new ElementCounts();
			foreach(var invader in Keys)
				clone[invader] = this[invader];
			return clone;
		}

		/// <summary> Reorders elements into 'Standard' order </summary>
		public string BuildElementString(string delimiter = " " ) {
			return this
				.OrderBy(p=>(int)p.Key)
				.Select(p=>p.Value+" "+p.Key.ToString().ToLower())
				.Join( delimiter ); // comma or space
		}

	}

}