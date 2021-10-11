
namespace SpiritIsland.BranchAndClaw {

	// !!! Use this Attribute on Innates for display purposes.
	public class SlowButFastIfAttribute : SpeedAttribute {
		readonly string fastTriggerElements;
		public SlowButFastIfAttribute(string fastTriggerElements) : base( Speed.Slow ) { this.fastTriggerElements = fastTriggerElements; }
		public override bool IsActiveFor( Speed requestSpeed, CountDictionary<Element> elements ) {
			return base.IsActiveFor( requestSpeed, elements )
				|| requestSpeed == Speed.Fast && elements.Contains(fastTriggerElements);
		} 
	}

}
