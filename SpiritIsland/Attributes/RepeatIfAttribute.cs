using System;

namespace SpiritIsland {

	// !! it would be nice if we could put this with the Innate Options
	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
	public class RepeatIfAttribute : System.Attribute {
		readonly string repeatTriggerElements;
		public RepeatIfAttribute(string fastTriggerElements) { this.repeatTriggerElements = fastTriggerElements; }
		public bool Repeat( CountDictionary<Element> elements ) {
			return elements.Contains(repeatTriggerElements);
		}
	}

}
