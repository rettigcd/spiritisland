using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// !! it would be nice if we could put this with the Innate Options
	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
	public class RepeatIfAttribute : System.Attribute {
		readonly CountDictionary<Element> repeatTriggerElements;
		public RepeatIfAttribute(string fastTriggerElements) { this.repeatTriggerElements = ElementList.Parse(fastTriggerElements); }
		public Task<bool> CanRepeat( Spirit spirit ) {
			return spirit.HasElements( repeatTriggerElements );
		}
	}

}
