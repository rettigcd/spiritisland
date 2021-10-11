
namespace SpiritIsland.BranchAndClaw {

	public class MinorCardConditinalFastAttribute : MinorCardAttribute {

		readonly string fastTriggerElements;
		public MinorCardConditinalFastAttribute( string name, int cost, string triggerElements, params Element[] elements )
			: base( name, cost, elements ) {
			this.fastTriggerElements = triggerElements;
		}

		public override void UpdateFromSpiritState( CountDictionary<Element> elements, PowerCard card ) {
			// if you have 3 air, this power may be fast
			card.OverrideSpeed = elements.Contains(fastTriggerElements) ? new SpeedOverride( Speed.FastOrSlow, "scour the land" ) : null;
		}

	}

}
