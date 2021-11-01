
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SlowButFastIfAttribute : SpeedAttribute {

		readonly CountDictionary<Element> triggerElements;
		public SlowButFastIfAttribute(string triggerElements) : base( Phase.Slow ) { this.triggerElements = ElementList.Parse( triggerElements ); }
		public override async Task<bool> IsActiveFor( Phase requestSpeed, Spirit spirit ) {
			return await base.IsActiveFor( requestSpeed, spirit )
				|| requestSpeed == Phase.Fast && await spirit.HasElements( triggerElements );
		}

		public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
			return base.CouldBeActiveFor( requestSpeed, spirit )
				|| requestSpeed == Phase.Fast && spirit.CouldHaveElements(triggerElements);
		}

	}

}
