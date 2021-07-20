using System;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public Element[] Elements { get; }

	}


}