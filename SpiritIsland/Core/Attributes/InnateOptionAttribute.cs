using System;
using System.Collections.Generic;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public InnateOptionAttribute(string elementFormat) {
			this.Elements = ParseElements( elementFormat );
		}

		public static Element[] ParseElements( string elementFormat ) {
			var items = new List<Element>();
			foreach(var singleElementType in elementFormat.Split( ',' )) {
				string[] parts = singleElementType.Trim().Split( ' ' );
				int count = int.Parse( parts[0] );
				Element el = (Element)Enum.Parse( typeof( Element ), parts[1], true );
				while(count-- > 0)
					items.Add( el );
			}

			return items.ToArray();
		}

		public Element[] Elements { get; }

	}


}