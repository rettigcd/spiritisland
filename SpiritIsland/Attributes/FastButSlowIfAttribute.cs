
namespace SpiritIsland {
	public class FastButSlowIfAttribute : SpeedAttribute {
		readonly string triggerElements;
		public FastButSlowIfAttribute(string triggerElements) : base( Speed.Fast ) { this.triggerElements = triggerElements; }
		public override bool IsActiveFor( Speed requestSpeed, CountDictionary<Element> elements ) {
			return base.IsActiveFor( requestSpeed, elements )
				|| requestSpeed == Speed.Slow && elements.Contains(triggerElements);
		}
	}

}
