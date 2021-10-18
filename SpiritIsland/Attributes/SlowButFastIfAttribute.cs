
namespace SpiritIsland {

	public class SlowButFastIfAttribute : SpeedAttribute {
		readonly string triggerElements;
		public SlowButFastIfAttribute(string triggerElements) : base( Speed.Slow ) { this.triggerElements = triggerElements; }
		public override bool IsActiveFor( Speed requestSpeed, CountDictionary<Element> elements ) {
			return base.IsActiveFor( requestSpeed, elements )
				|| requestSpeed == Speed.Fast && elements.Contains(triggerElements);
		}
	}

}
