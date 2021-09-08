using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public InnateOptionAttribute(string elementFormat) {
			this.Elements = ElementList.Parse( elementFormat );
		}

		public Element[] Elements { get; }

	}


}