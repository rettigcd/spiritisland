namespace SpiritIsland;

public class SlowButFastIfAttribute : SpeedAttribute {

	readonly ElementCounts triggerElements;
	public SlowButFastIfAttribute(string triggerElements) : base( Phase.Slow ) { this.triggerElements = ElementCounts.Parse( triggerElements ); }

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Fast && spirit.CouldHaveElements(triggerElements);
	}

}
