namespace SpiritIsland;

public class FastButSlowIfAttribute : SpeedAttribute {

	readonly ElementCounts triggerElements;
	public FastButSlowIfAttribute(string triggerElements) : base( Phase.Fast ) { this.triggerElements = ElementCounts.Parse(triggerElements); }

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Slow && spirit.CouldHaveElements(triggerElements);
	}


}